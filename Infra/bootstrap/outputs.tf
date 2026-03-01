output "resource_group_name" {
  value = azurerm_resource_group.main.name
}

output "resource_group_id" {
  value = azurerm_resource_group.main.id
}

output "storage_account_name" {
  value = azurerm_storage_account.tfstate.name
}

output "azure_tenant_id" {
  description = "Add this to GitHub Secrets as AZURE_TENANT_ID"
  value       = data.azurerm_client_config.current.tenant_id
}

output "azure_subscription_id" {
  description = "Add this to GitHub Secrets as AZURE_SUBSCRIPTION_ID"
  value       = data.azurerm_client_config.current.subscription_id
}

output "admin_object_id" {
  description = "Your Azure AD user object ID (for Key Vault admin access). Add to GitHub Secrets as KEY_VAULT_ADMIN_OBJECT_ID"
  value       = data.azurerm_client_config.current.object_id
}
