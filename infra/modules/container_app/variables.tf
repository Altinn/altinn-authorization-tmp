variable "infrastructure_name" {
  type    = string
  default = "auth"
}

variable "instance" {
  type    = string
  default = "001"
}

variable "location" {
  type        = string
  default     = "norwayeast"
  description = "Specifies the Azure region where the resources will be provisioned (e.g., 'norwayeast')."
}

variable "environment" {
  type = string
}

variable "name" {
  type = string
}

variable "image" {
  type = string
}

variable "registry" {
  type    = string
  default = "ghcr.io"
}

variable "can_use_service_bus" {
  type    = bool
  default = true
}

variable "max_replicas" {
  type    = number
  default = 5
}

variable "allocated_memory" {
  type    = number
  default = 0.5
}

variable "alloacated_cpu" {
  type    = number
  default = 0.25
}

variable "app_configuration_variables" {
  type    = map(string)
  default = {}
}
