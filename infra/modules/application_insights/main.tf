resource "azurerm_log_analytics_workspace" "log" {
  name                = "log${var.metadata.suffix}"
  resource_group_name = var.resource_group_name
  location            = var.location
  sku                 = "PerGB2018"
  retention_in_days   = 30

  tags = var.metadata
  lifecycle {
    prevent_destroy = true
    ignore_changes  = [tags]
  }
}

resource "azurerm_application_insights" "ai" {
  name                = "ai${var.metadata.suffix}"
  application_type    = "web"
  resource_group_name = var.resource_group_name
  location            = var.location

  tags = var.metadata
  lifecycle {
    prevent_destroy = true
    ignore_changes  = [tags]
  }
}
