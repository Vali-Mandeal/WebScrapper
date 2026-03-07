resource "azurerm_user_assigned_identity" "master" {
  name                = "id-func-master"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
}

# Master Function App - native Azure Functions on Container Apps (V2)
resource "azapi_resource" "function_master" {
  type      = "Microsoft.App/containerApps@2024-10-02-preview"
  name      = "ca-webscrapper-master"
  location  = data.azurerm_resource_group.main.location
  parent_id = data.azurerm_resource_group.main.id

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.master.id]
  }

  body = {
    kind = "functionapp"
    properties = {
      environmentId       = azurerm_container_app_environment.main.id
      workloadProfileName = "Consumption"
      configuration = {
        secrets = [
          {
            name  = "storage-connection"
            value = "DefaultEndpointsProtocol=https;AccountName=${azurerm_storage_account.functions.name};AccountKey=${azurerm_storage_account.functions.primary_access_key};EndpointSuffix=core.windows.net"
          },
          {
            name  = "appinsights-connection"
            value = azurerm_application_insights.main.connection_string
          },
          {
            name  = "ghcr-password"
            value = var.ghcr_token
          }
        ]
        registries = [
          {
            server            = "ghcr.io"
            username          = "Vali-Mandeal"
            passwordSecretRef = "ghcr-password"
          }
        ]
        ingress = {
          external   = true
          targetPort = 80
        }
      }
      template = {
        containers = [
          {
            name  = "function"
            image = var.master_image
            resources = {
              cpu    = 0.5
              memory = "1Gi"
            }
            env = [
              {
                name      = "AzureWebJobsStorage"
                secretRef = "storage-connection"
              },
              {
                name      = "APPLICATIONINSIGHTS_CONNECTION_STRING"
                secretRef = "appinsights-connection"
              },
              {
                name  = "FUNCTIONS_WORKER_RUNTIME"
                value = "dotnet-isolated"
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
        ]
        scale = {
          minReplicas = 0
          maxReplicas = 3
        }
      }
    }
  }
}
