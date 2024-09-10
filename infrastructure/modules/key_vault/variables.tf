variable "metadata" {
  type = object({
    name         = string
    environment  = string
    instance     = string
    suffix       = string
    default_tags = map(string)
  })
}

variable "resource_group_name" {
  type = string
}

variable "subnet_id" {
  type = string
}

variable "dns_zones" {
  type = list(string)
}
