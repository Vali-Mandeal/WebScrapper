resource "azurerm_storage_account" "functions" {
  name                     = "stwebscrapperfunc"
  resource_group_name      = data.azurerm_resource_group.main.name
  location                 = data.azurerm_resource_group.main.location
  account_tier             = "Standard"
  account_replication_type = "LRS"

  min_tls_version = "TLS1_2"
}

resource "azurerm_storage_queue" "scrap_jobs" {
  name                 = "scrap-jobs-queue"
  storage_account_name = azurerm_storage_account.functions.name
}
