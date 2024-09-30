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

variable "subnet_id" {
  type = string
}

variable "dns_zones" {
  type = list(string)
}

variable "key_vault_id" {
  type = string
}

variable "is_prod_like" {
  default = true
}

variable "permitted_ip_addresses" {
  default   = []
  type      = set(string)
  sensitive = true
}
