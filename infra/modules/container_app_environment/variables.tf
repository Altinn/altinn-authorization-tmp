variable "metadata" {
  type = object({
    name        = string
    environment = string
    instance    = string
    suffix      = string
    repository  = string
  })
  description = <<EOT
An object containing metadata used for resource identification and naming.
- name: The base name of the resource.
- environment: The deployment environment (e.g., dev, prod).
- instance: The specific instance identifier for the deployment.
- suffix: A suffix to create unique names for resources by combining name, instance, and environment.
- repository: The repository where the infrastructure code is stored.
EOT
}

variable "resource_group_name" {
  type        = string
  description = "The name of the Azure resource group where the container apps environment will be deployed."
}

variable "location" {
  type        = string
  description = "Specifies the Azure region where the container apps environment will be provisioned (e.g., 'norwayeast')."
}

variable "domains" {
  type        = map(string)
  description = <<EOT
A map of domain names for which private DNS 'A' records will be created. 
The keys are identifiers for the domains, and the values are the actual domain names.
EOT
}

variable "subnet_id" {
  type        = string
  description = "The ID of the subnet in which the Azure Container Apps environment will be deployed."
}

variable "vnet_id" {
  type        = string
  description = "The ID of the Virtual Network (VNet) where the subnet for the container apps environment is located."
}

variable "log_analytics_workspace_id" {
  type        = string
  description = "The ID of the Log Analytics workspace for logging and monitoring the Azure Container Apps environment."
}
