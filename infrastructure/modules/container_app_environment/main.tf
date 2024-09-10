data "azurerm_resource_group" "rg" {
  name = var.resource_group_name
}

resource "azurerm_container_app_environment" "environment" {
  name                       = "caenv${var.metadata.suffix}"
  resource_group_name        = data.azurerm_resource_group.rg.name
  location                   = data.azurerm_resource_group.rg.location
  log_analytics_workspace_id = var.log_analytics_workspace_id

  internal_load_balancer_enabled = true
  infrastructure_subnet_id       = var.subnet_id

  workload_profile {
    name                  = "basic"
    workload_profile_type = "D4"
    minimum_count         = 1
    maximum_count         = 5
  }

  tags = var.metadata.default_tags
}
