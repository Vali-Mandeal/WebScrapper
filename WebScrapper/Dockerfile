# Stage 1: Build the .NET function app and install Playwright
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Install Playwright CLI as a global tool
RUN dotnet tool install --global Microsoft.Playwright.CLI

# Ensure the .NET tools path is in the PATH environment variable
ENV PATH="$PATH:/root/.dotnet/tools"

# Set the working directory
WORKDIR /src/dotnet-function-app

# Copy the project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the remaining project files
COPY . .

# Build the project to ensure all assets are present
RUN dotnet build -c Release -o /src/dotnet-function-app/build

# Install Playwright browsers and dependencies
RUN playwright install --with-deps

# Publish the .NET function app
RUN dotnet publish -c Release -o /home/site/wwwroot

# Stage 2: Final runtime image with necessary dependencies for Playwright
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0

# Set environment variables for Azure Functions and Playwright
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    PLAYWRIGHT_BROWSERS_PATH=/ms-playwright

# Install system dependencies required by Playwright
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        libnss3 \
        libatk1.0-0 \
        libatk-bridge2.0-0 \
        libcups2 \
        libdrm2 \
        libxkbcommon0 \
        libgbm1 \
        libx11-xcb1 \
        libxcomposite1 \
        libxdamage1 \
        libxrandr2 \
        libasound2 \
        libpangocairo-1.0-0 \
        libgtk-3-0 \
        && rm -rf /var/lib/apt/lists/*

# Create directory for Playwright browsers
RUN mkdir -p /ms-playwright

# Copy Playwright browsers and dependencies from build stage
COPY --from=build-env /root/.cache/ms-playwright /ms-playwright

# Set permissions for Playwright browsers directory
RUN chmod -R 755 /ms-playwright

# Copy the published .NET function app from the build stage
COPY --from=build-env /home/site/wwwroot /home/site/wwwroot

