locals {
  postgres_server_sku = var.is_prod_like ? "GP_Standard_D4s_v3" : "B_Standard_B2ms"
}

# https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/user_assigned_identity
resource "azurerm_user_assigned_identity" "postgres_server_admin" {
  name                = "mipsqlsrvadmin${var.metadata.suffix}"
  resource_group_name = var.resource_group_name
  location            = var.location

  lifecycle {
    prevent_destroy = true
    ignore_changes  = [tags]
  }
}

# https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/postgresql_flexible_server
resource "azurerm_postgresql_flexible_server" "postgres_server" {
  name                = "psqlsrvaltinn${var.metadata.suffix}"
  resource_group_name = var.resource_group_name
  location            = var.location
  version             = var.postgres_version

  delegated_subnet_id           = var.subnet_id
  private_dns_zone_id           = var.dns_zone
  public_network_access_enabled = false

  storage_mb        = var.storage_mb
  auto_grow_enabled = true

  authentication {
    active_directory_auth_enabled = true
    password_auth_enabled         = false
    tenant_id                     = var.tenant_id
  }

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.postgres_server_admin.id]
  }

  sku_name = local.postgres_server_sku

  tags = var.metadata
  lifecycle {
    prevent_destroy = true
    ignore_changes  = [tags, zone]
  }
}

# https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/postgresql_flexible_server_active_directory_administrator
resource "azurerm_postgresql_flexible_server_active_directory_administrator" "admin" {
  server_name         = azurerm_postgresql_flexible_server.postgres_server.name
  resource_group_name = var.resource_group_name
  tenant_id           = var.tenant_id
  object_id           = azurerm_user_assigned_identity.postgres_server_admin.principal_id
  principal_name      = azurerm_user_assigned_identity.postgres_server_admin.name
  principal_type      = "ServicePrincipal"
}
