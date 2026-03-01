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

  # Master function - read-only secrets access
  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = azapi_resource.function_master.output.identity.principalId

    secret_permissions = ["Get", "List"]
  }

  # Slave function - read-only secrets access
  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = azapi_resource.function_slave.output.identity.principalId

    secret_permissions = ["Get", "List"]
  }
}
