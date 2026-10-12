locals {
  environments = ["at22", "at23", "at24", "yt01", "tt02", "prod"]


  feature_flags = flatten([
    for environment in local.environments : [
      for feature_flag in var.feature_flags : {
        name        = feature_flag.name
        default     = feature_flag.default
        label       = feature_flag.app == null ? environment : "${environment}-${app}"
        description = feature_flag.description
      }
    ]
  ])
}

# module "appsettings" {
#   source     = "../../modules/appsettings"
#   hub_suffix = local.suffix
# 
#   feature_flags = [for key, value in local.feature_flags :
#     {
#       default     = value.default
#       label       = value.label
#       description = value.description
#       name        = value.name
#   }]
# 
#   providers = {
#     azurerm.hub = azurerm
#   }
# }



module "appsettings" {
  source     = "../../modules/appsettings"
  hub_suffix = local.suffix

  feature_flags = [
    {
      default     = false
      description = "test"
      label       = "at22-access-management"
      name        = "test"
    }
  ]

  providers = {
    azurerm.hub = azurerm
  }
}

# id=https://appconfaltinnauth001hub.azconfig.io/kv/.appconfig.featureflag%2Ftest?label=at22-access-management
import {
  for_each = local.feature_flags

  to = module.appsettings.azurerm_app_configuration_feature.configuration[
    "${each.value.name}_${each.value.label}"
  ]

  id = "https://appconfaltinnauth001hub.azconfig.io/kv/.appconfig.featureflag%2F${each.value.name}?label=${each.value.label}"
}
