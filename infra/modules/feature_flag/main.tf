terraform {
  required_providers {
    azurerm = {
      source                = "hashicorp/azurerm"
      configuration_aliases = [azurerm.hub]
    }
  }
}

locals {
  configuration_store_id = "/subscriptions/${data.azurerm_client_config.hub.subscription_id}/resourceGroups/rg${var.hub_suffix}/providers/Microsoft.AppConfiguration/configurationStores/appconf${var.hub_suffix}"
  environments           = ["at22", "at23", "at24", "yt01", "tt02", "prod"]
  feature_flags          = { for environment in environments : environment => var.name }
}

variable "hub_suffix" {
  type    = string
  default = "altinnauth001"
}

variable "app" {
  type        = string
  description = "name of the app which will become the label <environment>-<app>"
}

variable "name" {
  type        = string
  description = "feature flag name"
}

variable "description" {
  type    = string
  default = "feature flag description"
}

data "azurerm_resource_group" "hub" {
  name     = "rg${var.hub_suffix}"
  provider = azurerm.hub
}

resource "azurerm_app_configuration_feature" "feature_flag" {
  configuration_store_id = local.configuration_store_id
  key                    = var.name
  name                   = var.name
  description            = var.description
  enabled                = false
  label                  = "${each.key}-${var.app}"
  provider               = azurerm.hub

  lifecycle {
    ignore_changes = [enabled]
  }

  for_each = local.feature_flags
}
