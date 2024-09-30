output "zones" {
  value = { for key, value in local.zones : key =>
    {
      id   = azurerm_private_dns_zone.dns[key].id
      name = value
    }
  }
  description = <<EOT
A map of all private link DNS zones. The keys are resource type names (e.g., "service_bus", "postgres"). Each value is an object with:
- id: The resource ID of the private DNS zone.
- name: The domain name for the private link associated with the resource.
EOT
}
