output "id" {
  value       = azurerm_virtual_network.vnet.id
  description = "Vnet ID"
}

output "name" {
  value       = azurerm_virtual_network.vnet.name
  description = "Vnet name"
}


# {
#   postgres : {
#     id = /subscriptions/{subscription_id}/resourceGroups/{resource_group_name}/virtualNetworks/{vnet_name}/subnets/postgres
#     name = postgres
#     address_prefixes = [
#       "10.202.1.0/28"
#     ]
#   }
#   ...
# }
output "subnets" {
  value = { for subnet in local.subnets : subnet.name =>
    {
      id               = azurerm_subnet.vnet[subnet.name].id
      name             = azurerm_subnet.vnet[subnet.name].name
      address_prefixes = [module.subnet.networks[index(module.subnet.networks.*.name, subnet.name)].cidr_block]
    }
  }

  description = "Dynamic object containing subnet name(s) as fields and objects with fields subnet id, name and address_prefixes"
}
