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

