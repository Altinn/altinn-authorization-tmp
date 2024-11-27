[private]
@default:
  just --list

# Install node packages required to run scripts - uses pnpm to install the packages
[private]
@install-script-packages:
  pushd .github/scripts
  pnpm install

[private]
@install-script-packages-frozen:
  pnpm install --frozen-lockfile

# Run the script to update solution files
@update-sln-files: install-script-packages-frozen
  ./.github/scripts/node_modules/.bin/tsx ./.github/scripts/update-sln-files.mts

# Print all projects metadata
@get-metadata: install-script-packages-frozen
  ./.github/scripts/node_modules/.bin/tsx ./.github/scripts/get-metadata.mts
