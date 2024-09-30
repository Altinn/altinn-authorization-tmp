output "connection_string" {
  sensitive   = true
  value       = azurerm_application_insights.ai.connection_string
  description = <<EOT
The connection string for the Azure Application Insights resource. 
This output is marked as sensitive, as it contains connection details needed to integrate Application Insights with other services or applications.
EOT
}

output "log_analytics_workspace_id" {
  value       = azurerm_log_analytics_workspace.log.id
  description = "The ID of the Azure Log Analytics workspace, which can be used for querying logs and metrics."
}
