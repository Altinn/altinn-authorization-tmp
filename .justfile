# Cross platform shebang:
shebang := if os() == 'windows' {
  'pwsh.exe'
} else {
  '/usr/bin/env pwsh'
}

# Set shell for non-Windows OSs:
set shell := ["pwsh", "-CommandWithArgs"]

# Set shell for Windows OSs:
set windows-shell := ["pwsh.exe", "-NoLogo", "-CommandWithArgs"]

[private]
@default:
  just --list

# Install node packages required to run scripts - uses pnpm to install the packages
[private]
@install-script-packages:
  #!{{shebang}}
  pushd .github/scripts
  pnpm install

[private]
@install-script-packages-frozen:
  #!{{shebang}}
  pushd .github/scripts
  pnpm install --frozen-lockfile

# Run the script to update solution files
@update-sln-files *ARGS: install-script-packages-frozen
  #!{{shebang}}
  node ./.github/scripts/update-sln-files.mts -- {{ARGS}}

@dev:
  #!{{shebang}}
  if ($IsWindows) {
    podman compose up -d
  } else {
    docker compose up -d
  }

# Print all projects metadata
@get-metadata: install-script-packages-frozen
  #!{{shebang}}
  node ./.github/scripts/get-metadata.mts

@db-cred:
  #!{{shebang}}
  dotnet run --project "./src/tools/Altinn.Authorization.Cli/src/Altinn.Authorization.Cli" -- db cred

@dev-pgsql-connection-string:
  #!{{shebang}}
  if ($IsWindows) {
    # On Windows (Podman), use the special DNS for host
    $port = $(podman inspect --format='{{"{{(index .NetworkSettings.Ports \"5432/tcp\" 0).HostPort}}"}}' altinn_authorization_postgres)
    Write-Output "Host=host.containers.internal;Port=$port;Username=admin;Password=admin;Database=accessmgmt"
  } else {
    $bridge_ip = $(ip a | grep docker0 | awk '/inet / {print $2}' | cut -d'/' -f1)
    $port = $(docker inspect --format='{{"{{(index .NetworkSettings.Ports \"5432/tcp\" 0).HostPort}}"}}' altinn_authorization_postgres)
    echo "Host=$bridge_ip;Port=$port;Username=admin;Password=admin;Database=accessmgmt"
  }

@dev-redis-cli-1:
  #!{{shebang}}
  if ($IsWindows) {
    redis-cli -h $(podman inspect --format='{{"{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}"}}' altinn_authorization_redis) -p 6379
  } else {
    redis-cli -h $(docker inspect --format='{{"{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}"}}' altinn_authorization_redis) -p 6379
  }

@dev-redis-cli-2:
  #!{{shebang}}
  redis-cli -h localhost -p 8002

@dev-clean:
  #!{{shebang}}
  if ($IsWindows) {
    podman compose rm -svf
  } else {
    docker compose rm -svf
  }
