data "azurerm_client_config" "current" {}

# https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/app_configuration
resource "azurerm_app_configuration" "app_configuration" {
  name                = "appconfaltinn${var.metadata.suffix}"
  resource_group_name = var.resource_group_name
  location            = var.location
  sku                 = lower("standard")
  local_auth_enabled  = false

  tags = var.metadata
  lifecycle {
    prevent_destroy = true
    ignore_changes  = [tags]
  }
}

# https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/role_assignment
resource "azurerm_role_assignment" "rbac" {
  scope                = azurerm_app_configuration.app_configuration.id
  principal_id         = data.azurerm_client_config.current.object_id
  role_definition_name = "App Configuration Data Owner" # https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#security
}

# https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/app_configuration_key
resource "azurerm_app_configuration_key" "key" {
  configuration_store_id = azurerm_app_configuration.app_configuration.id
  key                    = each.key
  value                  = each.value
  label                  = "default"

  tags = var.metadata
  lifecycle {
    ignore_changes = [tags]
  }

  for_each   = var.variables
  depends_on = [azurerm_role_assignment.rbac]
}
