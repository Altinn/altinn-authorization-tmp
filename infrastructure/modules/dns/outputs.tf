output "zones" {
  value = { for key, value in local.zones : key =>
    {
      id   = azurerm_private_dns_zone.dns[key].id
      name = value
    }
  }

  description = "Map of all private link DNS zones. The keys are the resource type name. e.g service_bus. Value contains the fields 'id' which is the ARM reference and name the domain"
}
