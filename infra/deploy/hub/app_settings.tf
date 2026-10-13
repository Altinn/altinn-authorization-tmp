locals {
  environments = ["at22", "at23", "at24", "yt01", "tt02", "prod"]

  feature_flags = flatten([
    for environment in local.environments : [
      for label, flags in var.feature_flags : [
        [for flag in flags :
          {
            label       = "${environment}-${label}"
            name        = flag.name
            description = flag.description
            default     = false
          }
        ]
      ]
    ]
  ])

  migrate_feature_flags = flatten([
    for environment in local.environments : [
      [
        {
          name        = "AccessManagementUI.DisplayPopularSingleRightsServices"
          description = "Whether or not to only display popular SingleRights services."
          label       = "${environment}-access-management-ui"
        },
        {
          name        = "AccessManagementUI.UseNewSingleRightsClientDelegation"
          description = "Whether or not to use the new version of client delegation for single rights services."
          label       = "${environment}-access-management-ui"
        },
        {
          name        = "AccessManagementUI.ShowHandledRequests"
          description = "Whether or not to show handled sent and received access package and resource requests."
          label       = "${environment}-access-management-ui"
        },
        {
          name        = "AccessManagementUI.ShowIdPortenAuthorizations"
          description = "Whether or not to include ID-porten authorizations in active consents list."
          label       = "${environment}-access-management-ui"
        },
        {
          name        = "AccessManagementUI.EnableSkyra"
          description = "Whether or not to load the Skyra survey SDK and ask for cookie consent."
          label       = "${environment}-access-management-ui"
        },
        {
          name        = "AccessManagementUI.EnableSingleRightsTabInPoaOverview"
          description = "Whether or not to show single rights tab in powers of attorney overview."
          label       = "${environment}-access-management-ui"
        }
      ]
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


# id=https://appconfaltinnauth001hub.azconfig.io/kv/.appconfig.featureflag%2Ftest?label=at22-access-management
import {
  for_each = toset(local.migrate_feature_flags)

  to = module.appsettings.azurerm_app_configuration_feature.configuration[
    "${each.value.name}_${each.value.label}"
  ]

  id = "https://appconfaltinnauth001hub.azconfig.io/kv/.appconfig.featureflag%2F${each.value.name}?label=${each.value.label}"
}
