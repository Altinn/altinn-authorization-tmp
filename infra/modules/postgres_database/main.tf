locals {
  infrastructure_suffix              = "${var.infrastructure_name}${var.instance}${var.environment}"
  infrastructure_resource_group_name = "rg${local.infrastructure_suffix}"
}

data "azurerm_postgresql_flexible_server" "server" {
  name                = "psqlsrv${local.infrastructure_suffix}"
  resource_group_name = local.infrastructure_resource_group_name
}

resource "azurerm_postgresql_flexible_server_database" "database" {
  name      = var.database_name
  server_id = data.azurerm_postgresql_flexible_server.server.id
  collation = "en_US.utf8"
  charset   = "utf8"

  # prevent the possibility of accidental data loss
  lifecycle {
    prevent_destroy = true
  }
}
