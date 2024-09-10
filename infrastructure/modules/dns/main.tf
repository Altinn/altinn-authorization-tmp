locals {
  # https://learn.microsoft.com/en-us/azure/private-link/private-endpoint-dns#virtual-network-and-on-premises-workloads-using-a-dns-forwarder
  zones = tomap({
    service_bus          = "privatelink.servicebus.windows.net"
    storage_account_blob = "privatelink.blob.core.windows.net"
    postgres             = "privatelink.postgres.database.azure.com"
    key_vault            = "privatelink.vaultcore.azure.net"
    app_configuration    = "privatelink.azconfig.io"
  })
}

data "azurerm_resource_group" "rg" {
  name = var.resource_group_name
}

# https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/dns_zone
resource "azurerm_private_dns_zone" "dns" {
  name                = each.value
  resource_group_name = data.azurerm_resource_group.rg.name

  for_each = local.zones
}

# https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/private_dns_zone_virtual_network_link
resource "azurerm_private_dns_zone_virtual_network_link" "dns" {
  name                  = each.key
  private_dns_zone_name = azurerm_private_dns_zone.dns[each.key].name

  virtual_network_id   = var.vnet_id
  resource_group_name  = data.azurerm_resource_group.rg.name
  registration_enabled = false

  for_each = local.zones
}
