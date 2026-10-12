locals {
  environments = ["at22", "at23", "at24", "yt01", "tt02", "prod"]

  feature_flags = flatten([
    for environment in local.environments : [
      for feature_flag in var.feature_flags : {
        (feature_flag.app == null ? "${environment}-${feature_flag.name}" : environment) = {
          default     = feature_flag.default
          name        = feature_flag.name
          label       = feature_flag.default
          description = feature_flag.description
        }
      }
    ]
  ])
}

module "appsettings" {
  source        = "../../modules/appsettings"
  hub_suffix    = local.suffix
  feature_flags = local.feature_flags

  providers = {
    azurerm.hub = azurerm
  }
}

import {
  for_each = local.feature_flags

  to = module.appsettings.azurerm_app_configuration_feature.configuration[
    "${each.value.name}_${each.value.label}"
  ]

  id = "https://appconfaltinnauth001hub.azconfig.io/kv/.appconfig.featureflag%2F${each.value.name}?label=${each.value.label}"
}
