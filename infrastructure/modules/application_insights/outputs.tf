output "instrumentation_key" {
  value = azurerm_application_insights.ai.instrumentation_key
}

output "log_analytics_workspace_id" {
  value = azurerm_log_analytics_workspace.log.id
}
