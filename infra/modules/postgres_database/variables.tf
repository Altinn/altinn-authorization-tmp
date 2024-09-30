variable "infrastructure_name" {
  type    = string
  default = "shared"
}

variable "instance" {
  type = string
}

variable "environment" {
  type = string
}

variable "database_name" {
  type     = string
  nullable = false
}
