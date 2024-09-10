locals {
  cidr_prefix = tonumber(split("/", var.cidr)[1])

  small_subnet  = 28 - local.cidr_prefix # Available IPs 11
  medium_subnet = 25 - local.cidr_prefix # Available IPs IPs 123
  large_subnet  = 23 - local.cidr_prefix # Available IPs 507

  ###! Important to not change order, resize or rename subnets once created and resource are allocated to the network.
  ###! For adding new subnets, append object only to the end of list.
  subnets = [
    {
      name : "default"
      bits = local.medium_subnet
    },
    {
      name : "container_apps"
      bits = local.large_subnet
      delegations = {
        fs = {
          name    = "Microsoft.App/environments"
          actions = []
        }
      }
    },
    {
      name : "postgres"
      bits = local.small_subnet
      delegations = {
        fs = {
          name = "Microsoft.DBforPostgreSQL/flexibleServers"
          actions = [
            "Microsoft.Network/virtualNetworks/subnets/join/action"
          ]
        }
      }
    }
  ]
}

data "azurerm_resource_group" "rg" {
  name = var.resource_group_name
}

resource "azurerm_virtual_network" "vnet" {
  name                = "vnet${var.metadata.suffix}"
  address_space       = [var.cidr]
  location            = data.azurerm_resource_group.rg.location
  resource_group_name = data.azurerm_resource_group.rg.name
}

module "subnet" {
  source          = "hashicorp/subnets/cidr"
  base_cidr_block = var.cidr
  networks = [for subnet in local.subnets : {
    name     = subnet.name
    new_bits = subnet.bits
  }]
}

resource "azurerm_subnet" "vnet" {
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.vnet.name
  address_prefixes     = [module.subnet.networks[index(module.subnet.networks.*.name, each.key)].cidr_block]
  name                 = each.key

  dynamic "delegation" {
    content {
      name = delegation.key
      service_delegation {
        name    = delegation.value.name
        actions = delegation.value.actions
      }
    }

    for_each = try(each.value.delegations, {})
  }

  for_each = { for subnet in local.subnets : subnet.name => subnet }
}
