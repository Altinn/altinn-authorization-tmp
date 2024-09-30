variable "metadata" {
  type = object({
    name        = string
    environment = string
    instance    = string
    suffix      = string
    repository  = string
  })
  description = <<EOT
An object containing metadata information used for resource identification and naming.
- name: The base name of the resource.
- environment: The environment in which the resource is being deployed (e.g., dev, prod).
- instance: A string representing the instance of the deployment.
- suffix: A suffix used to uniquely identify resources.
- repository: The URL of the repository where the infrastructure code is located.
EOT
}

variable "resource_group_name" {
  type        = string
  description = "The name of the Azure resource group where the private DNS zones and related resources will be deployed."
}

variable "domains" {
  type        = map(string)
  description = <<EOT
A map of custom domain names for additional private DNS zones. The key is the resource type (e.g., "mysql", "cosmosdb"), and the value is the private link domain (e.g., "privatelink.mysql.database.azure.com").
This allows users to add more private DNS zones for their services beyond the default ones.
EOT
}

variable "vnet_id" {
  type        = string
  description = "The ID of the virtual network to which the private DNS zones will be linked."
}
