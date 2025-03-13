variable "organization" {
  type = string
}

variable "product_name" {
  type = string
}

variable "instance" {
  type = string
}

variable "single_stack_ipv4_address_space" {
  type = string
}

variable "dual_stack_ipv4_address_space" {
  type = string
}

variable "dual_stack_ipv6_address_space" {
  type = string
}

variable "client_certs" {
  type = list(string)
}

variable "maintainers_principal_ids" {
  type = list(string)
}

variable "spoke_principal_ids" {
  type = list(string)
}

variable "developer_prod_principal_ids" {
  type = list(string)
}

variable "developer_dev_principal_ids" {
  type = list(string)
}

variable "vpn_owners_principal_ids" {
  type = list(string)
}

variable "hub_principal_ids" {
  type = list(string)
}

variable "vpn_routes" {
  type = map(list(string))
  default = {
    "AuthorizationInfrastructure" : [
      "10.202.0.0/16"
    ],
    "ServiceBusNoEast" = [
      # Use  "name": "ServiceBus.NorwayEast"
      # https://www.microsoft.com/en-us/download/details.aspx?id=56519"
      "51.13.0.128/26",
      "51.120.76.34/32",
      "51.120.83.200/32",
      "51.120.98.16/29",
      "51.120.106.128/29",
      "51.120.109.208/28",
      "51.120.210.128/29",
      "51.120.213.48/28",
      "51.120.237.64/26",
      "2603:1020:e04:1::220/123",
      "2603:1020:e04:3::500/120",
      "2603:1020:e04:402::170/125",
      "2603:1020:e04:802::150/125",
      "2603:1020:e04:c02::150/125"
    ]
  }
}


