output "postgres_server_name" {
  value       = data.azurerm_postgresql_flexible_server.auth.name
  description = "Name of the postgres server"
}

output "key_vault_name" {
  value       = data.azurerm_key_vault.auth.name
  description = "Name of the key vault"
}

output "resource_group_name" {
  value       = local.infrastructure_resource_group_name
  description = "Resource group name"
}

output "subscription_id" {
  value       = data.azurerm_client_config.current.subscription_id
  description = "Azure subscription ID"
}
