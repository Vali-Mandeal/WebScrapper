resource "azurerm_user_assigned_identity" "master" {
  name                = "id-func-master"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
}

# Master Function App - hosted on Container Apps (consumption pricing)
resource "azapi_resource" "function_master" {
  type      = "Microsoft.Web/sites@2023-12-01"
  name      = "func-webscrapper-master"
  location  = data.azurerm_resource_group.main.location
  parent_id = data.azurerm_resource_group.main.id

  body = {
    kind = "functionapp,linux,container,azurecontainerapps"
    identity = {
      type = "UserAssigned"
      userAssignedIdentities = {
        (azurerm_user_assigned_identity.master.id) = {}
      }
    }
    properties = {
      managedEnvironmentId = azurerm_container_app_environment.main.id
      workloadProfileName  = "Consumption"
      resourceConfig = {
        cpu    = 0.5
        memory = "1Gi"
      }
      siteConfig = {
        linuxFxVersion = "DOCKER|${var.master_image}"
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
            value = azurerm_user_assigned_identity.master.client_id
          }
        ]
      }
      clientAffinityEnabled = false
    }
  }
}
