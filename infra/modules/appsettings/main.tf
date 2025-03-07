terraform {
  required_providers {
    azurerm = {
      source                = "hashicorp/azurerm"
      configuration_aliases = [azurerm.hub]
    }
  }
}

data "azurerm_client_config" "hub" {
  provider = azurerm.hub
}

locals {
  configuration_store_id = "/subscriptions/${data.azurerm_client_config.hub.subscription_id}/resourceGroups/rg${var.hub_suffix}/providers/Microsoft.AppConfiguration/configurationStores/appconf${var.hub_suffix}"
}

data "azurerm_resource_group" "hub" {
  name     = "rg${var.hub_suffix}"
  provider = azurerm.hub
}

// Auth issue when looking up app conf
# data "azurerm_app_configuration" "app_configuration" {
#   name                = "appconf${var.hub_suffix}"
#   resource_group_name = data.azurerm_resource_group.hub.name
#   provider            = azurerm.hub
# }

resource "azurerm_app_configuration_feature" "configuration" {
  configuration_store_id = local.configuration_store_id
  key                    = each.value.name
  name                   = each.value.name
  description            = each.value.description
  enabled                = each.value.default
  label                  = each.value.label

  provider = azurerm.hub
  lifecycle {
    ignore_changes = [enabled]
  }

  for_each = { for flag in var.feature_flags : "${flag.name}_${flag.label}" => flag }
}

resource "azurerm_app_configuration_key" "key_value" {
  configuration_store_id = local.configuration_store_id
  type                   = "kv"
  key                    = each.value.key
  value                  = each.value.value
  label                  = each.value.label
  provider               = azurerm.hub

  for_each = { for flag in var.key_value : "${flag.key}_${flag.label}" => flag }
}

resource "azurerm_app_configuration_key" "key_vault_reference" {
  configuration_store_id = local.configuration_store_id
  type                   = "vault"
  key                    = each.value.key
  vault_key_reference    = each.value.key_vault_secret_id
  label                  = each.value.label
  provider               = azurerm.hub

  for_each = { for flag in var.key_vault_reference : "${flag.key}_${flag.label}" => flag }
}
