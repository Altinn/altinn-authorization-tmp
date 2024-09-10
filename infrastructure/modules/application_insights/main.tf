data "azurerm_resource_group" "rg" {
  name = var.resource_group_name
}

resource "azurerm_log_analytics_workspace" "log" {
  name                = "log${var.metadata.suffix}"
  resource_group_name = data.azurerm_resource_group.rg.name
  location            = data.azurerm_resource_group.rg.location
  sku                 = "PerGB2018"
  retention_in_days   = 30
}

resource "azurerm_application_insights" "ai" {
  name                = "ai${var.metadata.suffix}"
  application_type    = "web"
  resource_group_name = data.azurerm_resource_group.rg.name
  location            = data.azurerm_resource_group.rg.location
}
