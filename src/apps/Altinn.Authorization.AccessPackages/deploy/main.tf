terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.1.0"
    }
  }

  backend "azurerm" {
    use_azuread_auth = true
  }
}

provider "azurerm" {
  use_oidc = true
  features {}
}

module "app" {
  source = "../../../../infra/modules/container_app_api"

  instance    = var.instance
  environment = var.environment
  name        = "accesspackages"
  image       = var.image

  can_use_service_bus = true
}
