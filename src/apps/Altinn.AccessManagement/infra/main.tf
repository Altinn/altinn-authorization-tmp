terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.13.0"
    }
    static = {
      source  = "tiwood/static"
      version = "0.1.0"
    }
  }

  backend "azurerm" {
    use_azuread_auth = true
  }
}

provider "azurerm" {
  subscription_id = "37bac63a-b964-46b2-8de8-ba93c432ea1f"
  features {
  }
}


provider "azurerm" {
  alias           = "hub"
  subscription_id = var.hub_subscription_id
  features {
  }
}

locals {
  hub_suffix   = lower("${var.organization}${var.product_name}${var.instance}hub")
  spoke_suffix = lower("${var.organization}${var.product_name}${var.instance}${var.environment}")
  suffix       = lower("${var.organization}${var.product_name}${var.name}${var.instance}${var.environment}")

  default_tags = {
    ProductName = var.product_name
    Environment = var.environment
    Instance    = "001"
    Name        = var.name
    CreatedAt   = try(static_data.static.output.created_at, timestamp())
  }
}

resource "static_data" "static" {
  data = {
    created_at = formatdate("EEEE, DD-MMM-YY hh:mm:ss ZZZ", "2018-01-02T23:12:01Z")
  }

  lifecycle {
    ignore_changes = [data]
  }
}

data "azurerm_resource_group" "hub" {
  name     = "rg${local.hub_suffix}"
  provider = azurerm.hub
}

resource "azurerm_resource_group" "access_management" {
  name     = "rg${local.suffix}"
  location = "norwayeast"
  tags     = merge({}, local.default_tags)
}

resource "azurerm_user_assigned_identity" "access_management" {
  name                = "mi${local.suffix}"
  location            = azurerm_resource_group.access_management.location
  resource_group_name = azurerm_resource_group.access_management.name
  tags                = merge({}, local.default_tags)
}

resource "azurerm_federated_identity_credential" "aks_federation" {
  name                = "Aks"
  resource_group_name = azurerm_resource_group.access_management.name
  parent_id           = azurerm_user_assigned_identity.access_management.id

  audience = ["api://AzureADTokenExchange"]
  subject  = "system:serviceaccount:${each.value.namespace}:${each.value.service_account}"
  issuer   = each.value.issuer_url

  for_each = { for federation in var.aks_federation : federation.issuer_url => federation }
}

module "rbac" {
  source              = "../../../../infra/modules/rbac"
  principal_id        = azurerm_user_assigned_identity.access_management.principal_id
  hub_subscription_id = var.hub_subscription_id
  hub_suffix          = local.hub_suffix
  spoke_suffix        = local.spoke_suffix

  use_app_configuration = true
  use_lease             = true
  use_masstransit       = true
}

module "appsettings" {
  source              = "../../../../infra/modules/appsettings"
  hub_subscription_id = var.hub_subscription_id
  hub_suffix          = local.hub_suffix

  feature_flags = [
    {
      name        = "AccessManagement.SyncRegister"
      description = "Specifies if the register data should streamed from register service to access management database"
      label       = "${lower(var.environment)}_accessmanagement"
    }
  ]
}
