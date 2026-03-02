resource "azurerm_key_vault" "main" {
  name                = "kv-webscrapper"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  tenant_id           = data.azurerm_client_config.current.tenant_id
  sku_name            = "standard"

  soft_delete_retention_days = 7
  purge_protection_enabled   = false

  # Admin full access
  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = var.key_vault_admin_object_id

    secret_permissions = ["Get", "List", "Set", "Delete", "Purge"]
    key_permissions    = ["Get", "List", "Create", "Import", "Delete", "Update", "Backup", "Restore", "Recover", "Purge"]
  }
}

# Separate access policies to break the cycle (functions -> KV -> functions)
resource "azurerm_key_vault_access_policy" "master" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azapi_resource.function_master.output.identity.principalId

  secret_permissions = ["Get", "List"]
}

resource "azurerm_key_vault_access_policy" "slave" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azapi_resource.function_slave.output.identity.principalId

  secret_permissions = ["Get", "List"]
}
