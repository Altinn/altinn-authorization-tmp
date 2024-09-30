# https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/nat_gateway
resource "azurerm_nat_gateway" "nat_gateway" {
  name                    = "natgw${var.metadata.suffix}"
  resource_group_name     = var.resource_group_name
  location                = var.location
  sku_name                = "Standard"
  idle_timeout_in_minutes = 4

  tags = var.metadata
  lifecycle {
    prevent_destroy = true
    ignore_changes  = [tags]
  }
}

# https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/public_ip
resource "azurerm_public_ip" "nat_gateway" {
  name                 = "pipegress${var.metadata.suffix}"
  resource_group_name  = var.resource_group_name
  location             = var.location
  sku                  = "Standard"
  allocation_method    = "Static"
  ip_version           = "IPv4" # Nat gateway don't support IPv6 per. 27.09.24
  ddos_protection_mode = "VirtualNetworkInherited"

  tags = var.metadata

  lifecycle {
    prevent_destroy = true
    ignore_changes  = [tags]
  }
}

# https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/nat_gateway_public_ip_association
resource "azurerm_nat_gateway_public_ip_association" "nat_gateway" {
  nat_gateway_id       = azurerm_nat_gateway.nat_gateway.id
  public_ip_address_id = azurerm_public_ip.nat_gateway.id
}

# https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/subnet_nat_gateway_association
resource "azurerm_subnet_nat_gateway_association" "nat_gateway" {
  nat_gateway_id = azurerm_nat_gateway.nat_gateway.id
  subnet_id      = each.value.id

  for_each = { for subnet in var.subnets : subnet.name => subnet if subnet.nat_gateway }
}
