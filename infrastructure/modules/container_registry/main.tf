data "azurerm_resource_group" "rg" {
  name = var.resource_group_name
}

resource "azurerm_container_registry" "acr" {
  name                = "acr${var.metadata.suffix}"
  resource_group_name = var.resource_group_name
  location            = data.azurerm_resource_group.rg.location
  sku                 = "Standard"
  admin_enabled       = false

  anonymous_pull_enabled        = false
  public_network_access_enabled = true

  tags = var.metadata.default_tags
}
