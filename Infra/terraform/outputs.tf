output "master_function_name" {
  value = azapi_resource.function_master.name
}

output "slave_function_name" {
  value = azapi_resource.function_slave.name
}

output "key_vault_uri" {
  value = azurerm_key_vault.main.vault_uri
}

output "storage_account_name" {
  value = azurerm_storage_account.functions.name
}
