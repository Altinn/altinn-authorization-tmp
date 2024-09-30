variable "metadata" {
  type = object({
    name        = string
    environment = string
    instance    = string
    suffix      = string
    repository  = string
  })
  description = <<EOT
An object that contains metadata information used for resource identification and naming.

- name: The base name of the resource.
- environment: The environment in which the resources are being deployed (e.g., dev, prod).
- instance: A string representing the specific instance of the deployment.
- suffix: A suffix for unique resource naming, typically combining name, instance, and environment.
- repository: The URL of the repository where the infrastructure code is stored.
EOT
}

variable "resource_group_name" {
  type        = string
  description = "The name of the Azure resource group where the NAT gateway, public IP, and other resources will be deployed."
}

variable "location" {
  type        = string
  description = "Specifies the Azure region where the resources will be provisioned (e.g., 'norwayeast')."
}

variable "subnets" {
  type = map(object({
    name        = string
    id          = string
    nat_gateway = bool
  }))
  description = <<EOT
A map of subnets in which the NAT gateway may be associated. The key is the subnet name, and the value is an object with the following fields:

- name: The name of the subnet.
- id: The unique ID of the subnet.
- nat_gateway: A boolean flag that determines if the NAT gateway is associated with the subnet.
EOT
}
