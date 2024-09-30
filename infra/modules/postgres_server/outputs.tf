output "id" {
  value       = azurerm_postgresql_flexible_server.postgres_server.id
  description = "Postgres Flexible server AzureRM ID"
}

output "host" {
  value = azurerm_postgresql_flexible_server.postgres_server.fqdn
}

output "admin" {
  value       = azurerm_user_assigned_identity.postgres_server_admin.id
  description = "Managed Identity AzureRM ID"
}
