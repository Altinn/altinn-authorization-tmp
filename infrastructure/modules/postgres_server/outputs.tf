output "id" {
  value       = azurerm_key_vault_key.postgres_server.id
  description = "Postgres Flexible server AzureRM ID"
}

output "admin" {
  value       = azurerm_user_assigned_identity.postgres_server_admin.id
  description = "Managed Identity AzureRM ID"
}
