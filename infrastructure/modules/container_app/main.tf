resource "azurerm_container_app" "app" {
  name = "helloworld"

  container_app_environment_id = var.container_app_environment_id
  resource_group_name          = var.resource_group_name
  revision_mode                = "Single"

  ingress {
    allow_insecure_connections = false
    external_enabled           = true
    target_port                = 80
    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  registry {
    server = "docker.io"
  }

  template {
    container {
      name   = "test"
      image  = "hello-world:latest"
      cpu    = 0.25
      memory = "0.5Gi"
    }
  }

  tags = var.metadata.default_tags
}

