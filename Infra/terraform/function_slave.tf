resource "azurerm_user_assigned_identity" "slave" {
  name                = "id-func-slave"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
}

# Slave Function App - hosted on Container Apps (consumption pricing)
resource "azapi_resource" "function_slave" {
  type      = "Microsoft.Web/sites@2023-12-01"
  name      = "func-webscrapper-slave"
  location  = data.azurerm_resource_group.main.location
  parent_id = data.azurerm_resource_group.main.id

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.slave.id]
  }

  body = {
    kind = "functionapp,linux,container,azurecontainerapps"
    properties = {
      managedEnvironmentId = azurerm_container_app_environment.main.id
      workloadProfileName  = "Consumption"
      resourceConfig = {
        cpu    = 0.5
        memory = "1Gi"
      }
      siteConfig = {
        linuxFxVersion = "DOCKER|${var.slave_image}"
        cors = {
          allowedOrigins = ["https://portal.azure.com"]
        }
        appSettings = [
          {
            name  = "FUNCTIONS_EXTENSION_VERSION"
            value = "~4"
          },
          {
            name  = "APPLICATIONINSIGHTS_CONNECTION_STRING"
            value = azurerm_application_insights.main.connection_string
          },
          {
            name  = "AzureWebJobsStorage"
            value = "DefaultEndpointsProtocol=https;AccountName=${azurerm_storage_account.functions.name};AccountKey=${azurerm_storage_account.functions.primary_access_key};EndpointSuffix=core.windows.net"
          },
          {
            name  = "KeyVaultConfig__Url"
            value = azurerm_key_vault.main.vault_uri
          },
          {
            name  = "AZURE_CLIENT_ID"
            value = azurerm_user_assigned_identity.slave.client_id
          }
        ]
      }
      clientAffinityEnabled = false
    }
  }
}
