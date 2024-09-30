# Terraform Azure Key Vault & Managed Identity for Certificates

This project contains Terraform configurations to deploy an Azure Key Vault and associated resources to securely store SSL/TLS certificates. These certificates are required for securing both APIs and frontend services managed by an Azure Application Gateway. The certificates **must be issued by a trusted Norwegian Certificate Authority (CA)** and **must be uploaded to the Key Vault manually** before the Application Gateway can be deployed via a separate Terraform project (auth).

## Purpose

The primary purpose of this setup is to store the SSL/TLS certificates that will be used by an Azure Application Gateway to manage HTTPS traffic for both the APIs and frontend services. These certificates must exist in the Key Vault to allow the subsequent deployment of the Application Gateway infrastructure.

### Certificates

1. **API Certificate**: 
   - Used for securing the API endpoints.
   - The domain format is `api.auth.{environment}.altinn.cloud`.
   - This certificate must be uploaded to the Key Vault with the name **`api`**.

2. **Frontend Certificate**:
   - Used for securing the frontend services.
   - The domain format is `auth.{environment}.altinn.cloud`.
   - This certificate must be uploaded to the Key Vault with the name **`frontend`**.

These certificates are essential for enabling the HTTPS configuration of the Azure Application Gateway as it will receive traffic securely.

## Prerequisites

Before deploying this Terraform configuration, ensure you have the following:

1. **SSL/TLS Certificates**:
   - SSL/TLS certificates for both the API (`api`) and frontend (`frontend`) services.
   - Certificates **must be issued by a trusted Norwegian Certificate Authority (CA)**.
   - These certificates must be uploaded to the Azure Key Vault created by this project before the `auth` project can be deployed.

2. **Terraform**:
   - Terraform version 1.0.0 or later.

## Project Structure

- **`variables.tf`**: Defines configurable variables such as `environment`, `tenant_id`, `location`, and `instance`.
- **`main.tf`**: Main Terraform configuration that:
  - Configures the required Azure provider.
  - Defines local variables for resource naming and metadata.
  - Creates an Azure Resource Group and an Azure Key Vault for storing the certificates.
  - Creates a User-Assigned Managed Identity with the "Key Vault Administrator" role for managing certificates in the Key Vault.
