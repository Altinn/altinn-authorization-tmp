output "ip" {
  value       = azurerm_public_ip.nat_gateway.ip_address
  description = "The IP address of the public IP associated with the NAT gateway."
}
