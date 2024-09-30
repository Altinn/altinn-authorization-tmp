variable "metadata" {
  type = object({
    name        = string
    environment = string
    instance    = string
    suffix      = string
    repository  = string
  })
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

variable "prevent_destroy" {
  type    = bool
  default = true
}
