resource "azurerm_container_app_environment" "environment" {
  name                           = "caenv${var.metadata.suffix}"
  location                       = var.location
  resource_group_name            = var.resource_group_name
  log_analytics_workspace_id     = var.log_analytics_workspace_id
  infrastructure_subnet_id       = var.subnet_id
  internal_load_balancer_enabled = true
  zone_redundancy_enabled        = true

  workload_profile {
    name                  = "basic"
    workload_profile_type = "D4"
    minimum_count         = 1
    maximum_count         = 5
  }


  tags = var.metadata
  lifecycle {
    prevent_destroy = true
  }
}

resource "azurerm_private_dns_a_record" "asterisk" {
  name                = "*"
  zone_name           = each.value
  resource_group_name = var.resource_group_name
  ttl                 = 300
  records             = [azurerm_container_app_environment.environment.static_ip_address]

  tags = var.metadata
  lifecycle {
    prevent_destroy = false
  }

  for_each = var.domains
}
