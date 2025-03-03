organization                    = "altinn"
product_name                    = "auth"
instance                        = "001"
environment                     = "at23"
single_stack_ipv4_address_space = "10.202.96.0/21"
dual_stack_ipv4_address_space   = "10.202.112.0/20"
dual_stack_ipv6_address_space   = "fd0a:7204:c37f:300::/56"
hub_subscription_id             = "01de49cb-48ef-4494-bc9d-b9e19a90bcd5"
hub_principal_id                = "a9585a64-20f0-4d18-aba6-9930f92b809c"
prod_like                       = false
appsettings_key_value = {
  "Platform:ResourceRegistryEndpoint" : "http://altinn-resource-registry.default.svc.cluster.local"
  "Platform:RegisterEndpoint" : "http://altinn-register.default.svc.cluster.local"
}
spoke_principal_ids  = ["6eaed23e-df7f-4708-9c8e-a7f34deeadb4"]
platform_vnet_arm_id = "/subscriptions/de41df22-8dd0-435b-98dd-6152cd371e92/resourceGroups/altinnplatform-rg/providers/Microsoft.Network/virtualNetworks/at-platform-vnet"
