variable "metadata" {
  type = object({
    name        = string
    environment = string
    instance    = string
    suffix      = string
    repository  = string
  })
  description = <<EOT
An object that contains metadata information used for resource identification and naming conventions.

- name: The base name of the resources.
- environment: The environment (e.g., dev, prod) in which the resources are being deployed.
- instance: Represents the specific instance of the deployment (useful in multi-instance environments).
- suffix: A suffix used for unique resource naming, typically combining name, environment, and instance.
- repository: The Git repository where the infrastructure code is stored.
EOT
}

variable "location" {
  type        = string
  description = "Specifies the Azure region (e.g., 'norwayeast') where the virtual network and subnets will be provisioned."
}

variable "resource_group_name" {
  type        = string
  description = "The name of the Azure resource group where the virtual network and subnets will be created."
}

variable "ipv4_cidr" {
  type        = string
  description = "The Classless Inter-Domain Routing (CIDR) block for the virtual network, defining the IP address range (e.g., '10.202.0.0/20')."
}

variable "ipv6_cidr" {
  type        = string
  description = "The Classless Inter-Domain Routing (CIDR) block for the virtual network, defining the IP address range (e.g., 'fd39:ac82:d781::/48')."
}

variable "use_ipv6" {
  type    = bool
  default = false
}
