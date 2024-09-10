
variable "resource_group_name" {
  type = string
}

variable "postgres_server_name" {
  type = string
}

variable "database_name" {
  type     = string
  nullable = false
}
