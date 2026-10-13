# Feature flags for the Access Management frontend BFF
# (https://github.com/Altinn/altinn-access-management-frontend).
# The frontend reads these from the hub App Configuration store using the
# "<env>-access-management-ui" label, keeping them separate from the backend
# flags in main.tf. Defined here until the frontend moves into this repo.
module "frontend_appsettings" {
  source     = "../../../../infra/modules/appsettings"
  hub_suffix = local.hub_suffix

  providers = {
    azurerm.hub = azurerm.hub
  }
}

removed {
  from = module.frontend_appsettings.azurerm_app_configuration_feature.configuration

  lifecycle {
    destroy = false
  }
}
