terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.115.0"
    }
    random = {
      source  = "hashicorp/random"
      version = ">= 3.4.0"
    }
  }

  # backend "azurerm" {
  #   use_azuread_auth = true
  # }
}

locals {
  repository  = "github.com/altinn/altinn-authorization"
  environment = lower(var.environment)
  name        = "shared"

  metadata = {
    name        = local.name
    environment = local.environment
    instance    = var.instance
    suffix      = "${local.name}${var.instance}${var.environment}"
    default_tags = {
      repository = local.repository
    }
  }
}

resource "azurerm_resource_group" "shared" {
  name     = "rg${local.metadata.suffix}"
  location = var.location
}

module "vnet" {
  source              = "../modules/vnet"
  resource_group_name = azurerm_resource_group.shared.name
  metadata            = local.metadata

  cidr = var.cidr

  depends_on = [azurerm_resource_group.shared]
}

module "nat" {
  source              = "../modules/nat_gateway"
  resource_group_name = azurerm_resource_group.shared.name
  metadata            = local.metadata

  subnets = module.vnet.subnets

  depends_on = [azurerm_resource_group.shared]
}

module "dns" {
  source              = "../modules/dns"
  resource_group_name = azurerm_resource_group.shared.name
  metadata            = local.metadata

  vnet_id = module.vnet.id

  depends_on = [azurerm_resource_group.shared]
}

module "key_vault" {
  source              = "../modules/key_vault"
  resource_group_name = azurerm_resource_group.shared.name
  metadata            = local.metadata

  dns_zones = [module.dns.zones["key_vault"].id]
  subnet_id = module.vnet.subnets["default"].id

  depends_on = [azurerm_resource_group.shared]
}

module "service_bus" {
  source              = "../modules/service_bus"
  resource_group_name = azurerm_resource_group.shared.name
  metadata            = local.metadata

  is_prod_like           = true
  key_vault_id           = module.key_vault.id
  dns_zones              = [module.dns.zones["service_bus"].id]
  subnet_id              = module.vnet.subnets["default"].id
  permitted_ip_addresses = [module.nat.ip]

  depends_on = [azurerm_resource_group.shared, module.key_vault]
}

module "postgres_server" {
  source              = "../modules/postgres_server"
  resource_group_name = azurerm_resource_group.shared.name
  metadata            = local.metadata

  dns_zone     = module.dns.zones["postgres"].id
  key_vault_id = module.key_vault.id
  subnet_id    = module.vnet.subnets["postgres"].id

  depends_on = [azurerm_resource_group.shared, module.key_vault]
}

module "application_insights" {
  source              = "../modules/application_insights"
  resource_group_name = azurerm_resource_group.shared.name
  metadata            = local.metadata
  depends_on          = [azurerm_resource_group.shared]
}

module "container_app_environment" {
  source              = "../modules/container_app_environment"
  resource_group_name = azurerm_resource_group.shared.name
  metadata            = local.metadata

  subnet_id                  = module.vnet.subnets["container_apps"].id
  log_analytics_workspace_id = module.application_insights.log_analytics_workspace_id

  depends_on = [azurerm_resource_group.shared, module.key_vault]
}

module "azurerm_container_app" {
  source              = "../modules/container_app"
  resource_group_name = azurerm_resource_group.shared.name
  metadata            = local.metadata

  container_app_environment_id = module.container_app_environment.id

  depends_on = [azurerm_resource_group.shared]
}
