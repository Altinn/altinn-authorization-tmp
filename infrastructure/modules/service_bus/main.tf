# NOTES
# * You only see the Networking tab for premium namespaces. To set IP firewall rules for the other tiers, use Azure Resource Manager templates, Azure CLI, PowerShell or REST API.
# * Private endpoint and CMK encryption are only available in Premium SKU
# * Capacity and Partitions are only allowed to set if SKU is premium
# * Using User Assigned Identity due to problematic execution order for System assigned identity and enabling CMK encryption

locals {
  service_bus_enable_public_endpoint       = !var.is_prod_like
  service_bus_sku                          = var.is_prod_like ? "Premium" : "Standard"
  service_bus_enable_local_auth            = !var.is_prod_like
  service_bus_enable_private_endpoint      = var.is_prod_like         # Only avaiable for Premium tier
  service_bus_enable_encryption_at_rest    = var.is_prod_like         # Only avaiable for Premium tier
  service_bus_capacity                     = var.is_prod_like ? 1 : 0 # Only avaiable for Premium tier
  service_bus_premium_messaging_partitions = var.is_prod_like ? 1 : 0 # Only avaiable for Premium tier
}

data "azurerm_resource_group" "rg" {
  name = var.resource_group_name
}

# https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#security
data "azurerm_role_definition" "key_vault_crypto_officer" {
  role_definition_id = "14b46e9e-c2b7-41b4-b07b-48a6ebf60603"
}


# https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/user_assigned_identity
resource "azurerm_user_assigned_identity" "service_bus" {
  name                = "misb${var.metadata.suffix}"
  resource_group_name = data.azurerm_resource_group.rg.name
  location            = data.azurerm_resource_group.rg.location
}

# https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/role_assignment
resource "azurerm_role_assignment" "key_vault_crypto_officer" {
  scope                            = var.key_vault_id
  principal_id                     = azurerm_user_assigned_identity.service_bus.principal_id
  role_definition_name             = data.azurerm_role_definition.key_vault_crypto_officer.name
  skip_service_principal_aad_check = true
}

# https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/key_vault_key
resource "azurerm_key_vault_key" "service_bus" {
  name         = "sb${var.metadata.suffix}"
  key_vault_id = var.key_vault_id
  key_type     = "RSA"
  key_size     = 2048

  count = local.service_bus_enable_private_endpoint ? 1 : 0

  key_opts = [
    "unwrapKey",
    "wrapKey"
  ]
}

# https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/servicebus_namespace
resource "azurerm_servicebus_namespace" "service_bus" {
  name                         = "sb${var.metadata.suffix}"
  resource_group_name          = data.azurerm_resource_group.rg.name
  location                     = data.azurerm_resource_group.rg.location
  sku                          = local.service_bus_sku
  local_auth_enabled           = local.service_bus_enable_local_auth
  capacity                     = local.service_bus_capacity
  premium_messaging_partitions = local.service_bus_premium_messaging_partitions

  dynamic "customer_managed_key" {
    content {
      infrastructure_encryption_enabled = true
      identity_id                       = azurerm_user_assigned_identity.service_bus.id
      key_vault_key_id                  = azurerm_key_vault_key.service_bus[0].id
    }

    for_each = local.service_bus_enable_encryption_at_rest ? [1] : []
  }

  network_rule_set {
    default_action                = "Deny"
    public_network_access_enabled = local.service_bus_enable_public_endpoint
    ip_rules                      = var.permitted_ip_addresses
    trusted_services_allowed      = true
  }

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.service_bus.id]
  }

  depends_on = [azurerm_role_assignment.key_vault_crypto_officer]
}

# https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/private_endpoint
resource "azurerm_private_endpoint" "service_bus_private_endpoint" {
  name                          = "pe${azurerm_servicebus_namespace.service_bus.name}"
  location                      = data.azurerm_resource_group.rg.location
  resource_group_name           = data.azurerm_resource_group.rg.name
  subnet_id                     = var.subnet_id
  custom_network_interface_name = "nic${azurerm_servicebus_namespace.service_bus.name}"

  count = local.service_bus_enable_private_endpoint ? 1 : 0

  private_service_connection {
    name                           = azurerm_servicebus_namespace.service_bus.name
    private_connection_resource_id = azurerm_servicebus_namespace.service_bus.id
    is_manual_connection           = false
    subresource_names              = ["namespace"]
  }

  private_dns_zone_group {
    name                 = azurerm_servicebus_namespace.service_bus.name
    private_dns_zone_ids = var.dns_zones
  }
}

# Service bus Actions List: https://learn.microsoft.com/en-us/azure/role-based-access-control/permissions/integration#microsoftservicebus
# https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/role_definition
resource "azurerm_role_definition" "service_bus_masstransit" {
  name        = "Azure Service Bus Mass Transit"
  scope       = azurerm_servicebus_namespace.service_bus.id
  description = "Allow C# Applications use MassTransit with Azure Service Bus"

  permissions {
    actions = [
      "Microsoft.ServiceBus/namespaces/read",
      "Microsoft.ServiceBus/namespaces/queues/*",
      "Microsoft.ServiceBus/namespaces/topics/*"
    ]
  }

  assignable_scopes = [azurerm_servicebus_namespace.service_bus.id]
}
