variable "metadata" {
  type = object({
    name        = string
    environment = string
    instance    = string
    suffix      = string
    repository  = string
  })
}

variable "entraid_admins" {
  type        = map(string)
  default     = {}
  description = "List of objects IDs"
}

variable "resource_group_name" {
  type = string
}

variable "location" {
  type = string
}

variable "tenant_id" {
  type = string
}

variable "subnet_id" {
  type = string
}

variable "dns_zones" {
  type = list(string)
}
