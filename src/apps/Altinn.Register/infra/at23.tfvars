environment = "at23"
aks_federation = [
  {
    issuer_url      = "https://norwayeast.oic.prod-aks.azure.com/cd0026d8-283b-4a55-9bfa-d0ef4a8ba21c/68c4e940-5ccb-4825-9eb5-2bb429e242b5/"
    namespace       = "default"
    service_account = "altinn-register"
  }
]
deploy_app_principal_id = "6eaed23e-df7f-4708-9c8e-a7f34deeadb4"
platform_workflow_principal_ids = [
  "143d9d84-6ef6-4c60-a2a5-9cb232dd2268", # altinn-register-app-at23-aks01
  "e785218c-e5a5-4c8d-b1df-f262b8eaa241"  # altinn-register-app-at23-aks02
]
db_max_pool_size = 4
db_compute_tier  = "Burstable"
db_compute_size  = "Standard_B2s"
