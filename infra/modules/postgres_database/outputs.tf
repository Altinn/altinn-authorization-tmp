output "database" {
  value = azurerm_postgresql_flexible_server_database.database.name
}

output "host" {
  value = data.azurerm_postgresql_flexible_server.server.fqdn
}
