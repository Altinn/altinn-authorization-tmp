

data "azurerm_resource_group" "rg" {
  name = var.resource_group_name
}

data "azurerm_postgresql_flexible_server" "postgres_database" {
  name                = var.postgres_server_name
  resource_group_name = data.azurerm_resource_group.rg.name
}

resource "azurerm_postgresql_flexible_server_database" "postgres_database" {
  name      = var.database_name
  server_id = azurerm_postgresql_flexible_server.postgres_database.id
  collation = "en_US.utf8"
  charset   = "utf8"

  # prevent the possibility of accidental data loss
  lifecycle {
    prevent_destroy = true
  }
}
