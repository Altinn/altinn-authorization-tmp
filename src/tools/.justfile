# Set shell for non-Windows OSs:
set shell := ["pwsh", "-c"]

# Set shell for Windows OSs:
set windows-shell := ["pwsh.exe", "-NoLogo", "-Command"]

set dotenv-load := true

[private]
@default:
  just --choose

@register-export-errors ENV='at22':
  dotnet run --project './Altinn.Authorization.Cli/src/Altinn.Authorization.Cli' --no-launch-profile -- sb export-errors 'sb://sbaltinnauth001{{ENV}}.servicebus.windows.net?tenantId=${ALTINN_ENV_{{uppercase(ENV)}}_AZURE_TENANT_ID}'

@register-retry-errors ENV='at22':
  dotnet run --project './Altinn.Authorization.Cli/src/Altinn.Authorization.Cli' --no-launch-profile -- register retry 'sb://sbaltinnauth001{{ENV}}.servicebus.windows.net?tenantId=${ALTINN_ENV_{{uppercase(ENV)}}_AZURE_TENANT_ID}' '{kv:kvregaltinnauth001{{ENV}}/db-psqlsrvaltinnauthregister001{{ENV}}-register-app}'
