locals {
  environments = ["at22", "at23", "at24", "yt01", "tt02", "prod"]


  feature_flags = flatten([
    for environment in local.environments : [
      for feature_flag in var.feature_flags : {
        environment = environment
        name        = feature_flag.name
        default     = feature_flag.default
        label       = environment
        description = feature_flag.description
      }
    ]
  ])
}

module "appsettings" {
  source     = "../../modules/appsettings"
  hub_suffix = local.suffix

  feature_flags = [for key, value in local.feature_flags :
    {
      default     = value.default
      label       = value.label
      description = value.description
      name        = value.name
  }]

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
