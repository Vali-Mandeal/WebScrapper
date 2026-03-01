#!/usr/bin/env bash
set -euo pipefail

# This script creates the Azure AD resources needed for GitHub Actions OIDC authentication.
# Run this after `terraform apply` in the bootstrap directory.
#
# Prerequisites:
#   - Azure CLI logged in (`az login`)
#   - Terraform bootstrap already applied (resource group must exist)
#
# Usage:
#   ./setup-github-oidc.sh <github_repo> [resource_group_name]
#
# Example:
#   ./setup-github-oidc.sh "valimandeal/WebScrapper" "rg-webscrapper"

GITHUB_REPO="${1:?Usage: $0 <github_repo> [resource_group_name]}"
RESOURCE_GROUP="${2:-rg-webscrapper}"
APP_NAME="${3:-webscrapper-github-actions}"

echo "==> Creating Azure AD App Registration: ${APP_NAME}"
APP_ID=$(az ad app create --display-name "${APP_NAME}" --query "appId" -o tsv)
echo "    App ID (Client ID): ${APP_ID}"

echo "==> Creating Service Principal"
SP_OBJECT_ID=$(az ad sp create --id "${APP_ID}" --query "id" -o tsv)
echo "    Service Principal Object ID: ${SP_OBJECT_ID}"

echo "==> Creating Federated Identity Credential for repo: ${GITHUB_REPO} (main branch)"
az ad app federated-credential create \
  --id "${APP_ID}" \
  --parameters "{
    \"name\": \"github-actions-main\",
    \"issuer\": \"https://token.actions.githubusercontent.com\",
    \"subject\": \"repo:${GITHUB_REPO}:ref:refs/heads/main\",
    \"audiences\": [\"api://AzureADTokenExchange\"]
  }" \
  --output none

echo "==> Assigning Contributor role on resource group: ${RESOURCE_GROUP}"
RG_ID=$(az group show --name "${RESOURCE_GROUP}" --query "id" -o tsv)
az role assignment create \
  --assignee-object-id "${SP_OBJECT_ID}" \
  --assignee-principal-type ServicePrincipal \
  --role "Contributor" \
  --scope "${RG_ID}" \
  --output none

echo "==> Assigning Key Vault Contributor role on resource group: ${RESOURCE_GROUP}"
az role assignment create \
  --assignee-object-id "${SP_OBJECT_ID}" \
  --assignee-principal-type ServicePrincipal \
  --role "Key Vault Contributor" \
  --scope "${RG_ID}" \
  --output none

TENANT_ID=$(az account show --query "tenantId" -o tsv)
SUBSCRIPTION_ID=$(az account show --query "id" -o tsv)

echo ""
echo "=============================="
echo " Add these to GitHub Secrets: "
echo "=============================="
echo "  AZURE_CLIENT_ID:       ${APP_ID}"
echo "  AZURE_TENANT_ID:       ${TENANT_ID}"
echo "  AZURE_SUBSCRIPTION_ID: ${SUBSCRIPTION_ID}"
echo ""
echo "Done!"
