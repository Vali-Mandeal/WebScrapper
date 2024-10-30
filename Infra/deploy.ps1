# deploy.ps1
. "$PSScriptRoot\parameters.ps1"

function Check-AzureCLI {
    if (-not (Get-Command "az" -ErrorAction SilentlyContinue)) {
        Write-Error "Azure CLI is not installed. Please install it from https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
        exit 1
    }
}

function DockerHub-Login {
    Write-Host "Logging into DockerHub..."
    echo $dockerHubPassword | docker login --username $dockerHubUsername --password-stdin
    Write-Host "DockerHub login successful."
}

function Docker-BuildPush {
    Write-Host "Navigating to project directory: $PSScriptRoot"
    Set-Location -Path $PSScriptRoot

    Write-Host "Building Docker image using docker-compose..."
    docker-compose build

    Write-Host "Tagging Docker image as $fullImageName..."
    docker tag $initialImageName $fullImageName

    Write-Host "Pushing Docker image to DockerHub..."
    docker push $fullImageName
    Write-Host "Docker image pushed successfully."
}

function Deploy-ARMTemplate {
    Write-Host "Authenticating with Azure CLI..."
    az login --output none
    Write-Host "Azure CLI authenticated."
    az account set --subscription $subscriptionId --output none

    Write-Host "Creating Resource Group: $resourceGroup in $location..."
    az group create --name $resourceGroup --location "$location" --output none

    Write-Host "Deploying ARM template with parameters from $parametersFile..."
    az deployment group create `
        --resource-group $resourceGroup `
        --template-file "$PSScriptRoot\$templateFile" `
        --parameters "$PSScriptRoot\$parametersFile" `
        --output none

    Write-Host "ARM template deployed successfully."
}

# Main Execution Flow
try {
    Check-AzureCLI
    DockerHub-Login
    Docker-BuildPush
    Deploy-ARMTemplate
    Write-Host "CI/CD Pipeline executed successfully."
} catch {
    Write-Error "Deployment failed: $_"
    exit 1
}
