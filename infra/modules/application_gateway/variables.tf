variable "metadata" {
  type = object({
    name        = string
    environment = string
    instance    = string
    suffix      = string
    repository  = string
  })
  description = <<EOT
An object containing metadata for resource identification and naming.
- name: The base name of the resource.
- environment: The deployment environment (e.g., dev, prod).
- instance: The specific instance identifier for the deployment.
- suffix: A suffix to create unique names for resources by combining name, instance, and environment.
- repository: The repository where the infrastructure code is stored.
EOT
}

variable "zones" {
  type        = list(string)
  default     = ["1"]
  description = "A list of availability zones for the Application Gateway. If empty, no zones will be assigned."
}

variable "cert_resource_group_name" {
  type        = string
  description = "The name of the Azure resource group that contains the Key Vault for SSL certificates."
}

variable "cert_keyvault_name" {
  type        = string
  description = "The name of the Key Vault that holds the SSL certificates for the Application Gateway."
}

variable "cert_user_assigned_identity_name" {
  type        = string
  description = "The name of the user-assigned managed identity to be used for accessing the Key Vault."
}

variable "resource_group_name" {
  type        = string
  description = "The name of the Azure resource group where the Application Gateway will be deployed."
}

variable "location" {
  type        = string
  description = "The Azure region where the Application Gateway will be provisioned (e.g., 'norwayeast')."
}

variable "subnet_id" {
  type        = string
  description = "The ID of the subnet in which the Application Gateway will be deployed."
}

variable "domains" {
  type = object({
    api      = string
    frontend = string
  })
  description = <<EOT
An object containing domain names for the Application Gateway.
- api: The domain name for the API backend.
- frontend: The domain name for the frontend application.
EOT
}

variable "log_analytics_workspace_id" {
  type        = string
  description = "The ID of the Log Analytics workspace for logging and monitoring the Application Gateway."
}

variable "services" {
  type = list(object({
    domain   = string
    path     = string
    hostname = string
  }))
  description = <<EOT
A list of backend services that the Application Gateway will route requests to.
Each object includes:
- domain: The domain associated with the backend service.
- path: The path to the backend service.
- hostname: The hostname of the backend service.
EOT
}

variable "max_capacity" {
  type        = number
  default     = 10
  description = "The maximum number of instances that the Application Gateway can scale to."
}
