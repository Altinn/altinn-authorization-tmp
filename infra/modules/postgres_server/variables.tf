
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

variable "key_vault_id" {
  type = string
}

variable "subnet_id" {
  type = string
}

variable "is_prod_like" {
  type = bool
}

variable "dns_zone" {
  type        = string
  description = "Specifies if DNS should be resolved internally or not. If specifies public endpoint is disabled"
}

variable "postgres_version" {
  default = "16"
  type    = string
}

variable "storage_mb" {
  default = 32768
  type    = number
  validation {
    condition     = contains([32768, 65536, 131072, 262144, 524288, 1048576, 2097152, 4193280, 4194304, 8388608, 16777216, 33553408], var.storage_mb)
    error_message = "possible values for 32768, 65536, 131072, 262144, 524288, 1048576, 2097152, 4193280, 4194304, 8388608, 16777216, 33553408"
  }
}

