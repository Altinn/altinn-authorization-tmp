organization                    = "altinn"
product_name                    = "auth"
instance                        = "001"
environment                     = "yt01"
single_stack_ipv4_address_space = "10.202.160.0/21"
dual_stack_ipv4_address_space   = "10.202.176.0/20"
dual_stack_ipv6_address_space   = "fd0a:7204:c37f:500::/56"
hub_subscription_id             = "01de49cb-48ef-4494-bc9d-b9e19a90bcd5"
hub_principal_id                = "a9585a64-20f0-4d18-aba6-9930f92b809c"
prod_like                       = true

spoke_principal_ids = ["6eaed23e-df7f-4708-9c8e-a7f34deeadb4"]
service_bus_firewall = [
  "51.13.29.120/31",
  "51.13.29.32/31"
]

# app-configuration
platform_resource_registry_endpoint = "http://altinn-resource-registry.default.svc.cluster.local"
platform_register_endpoint          = "http://altinn-register.default.svc.cluster.local"
platform_sbl_endpoint               = "https://ai-yt01-vip-sblbridge.ai.basefarm.net/sblbridge/"
