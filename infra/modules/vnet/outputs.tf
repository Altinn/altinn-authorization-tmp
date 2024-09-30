output "id" {
  value       = azurerm_virtual_network.vnet.id
  description = "The ID of the created virtual network."
}

output "name" {
  value       = azurerm_virtual_network.vnet.name
  description = "The name of the created virtual network."
}

output "subnets" {
  value = { for subnet in local.subnets : subnet.name =>
    {
      id          = azurerm_subnet.vnet[subnet.name].id
      name        = azurerm_subnet.vnet[subnet.name].name
      nat_gateway = local.subnets[index(local.subnets.*.name, subnet.name)].nat_gateway
      ipv4        = [module.ipv4.networks[index(module.ipv4.networks.*.name, subnet.name)].cidr_block]
      ipv6        = [module.ipv6.networks[index(module.ipv6.networks.*.name, subnet.name)].cidr_block]
    }
  }

  description = <<EOT
A dynamic object containing subnet details. Each field is a subnet name, and the corresponding value is an object with the following fields:
- id: The ID of the subnet.
- name: The name of the subnet. (default, application_gateway, container_apps, postgres)
- nat_gateway: A boolean indicating whether the NAT gateway is enabled for this subnet.
- IPV4: The IPV4 address prefixes assigned to the subnet.
- IPV6: The IPV6 address prefixes assigned to the subnet.
EOT
}
