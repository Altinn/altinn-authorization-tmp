output "id" {
  value = azurerm_servicebus_namespace.service_bus.id
}

output "host" {
  value = azurerm_servicebus_namespace.service_bus.endpoint
}
