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
  - instance: A string representing the instance of the deployment, used to differentiate between multiple deployments of the same resource.
  - suffix: A suffix for unique resource naming, typically combining name, instance, and environment.
  - repository: The repository URL where the infrastructure code resides.
EOT
}

variable "resource_group_name" {
  type        = string
  description = "The name of the Azure resource group where the resources will be deployed."
}

variable "location" {
  type        = string
  description = "Specifies the Azure region where the resources will be provisioned (e.g., 'norwayeast')."
}

variable "variables" {
  type        = map(string)
  description = "A map of configuration variables that are stored in the Azure App Configuration store. The map key is the variable name, and the value is the corresponding configuration value."
  default     = {}
}
