# Stage 1: Build the .NET function app and install Playwright
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Install PowerShell (required by Playwright install script)
RUN dotnet tool install --global PowerShell
ENV PATH="$PATH:/root/.dotnet/tools"

WORKDIR /src

# Copy project files and restore
COPY WebScrapper.Shared/WebScrapper.Shared.csproj WebScrapper.Shared/
COPY WebScrapper.Slave/WebScrapper.Slave.csproj WebScrapper.Slave/
RUN dotnet restore WebScrapper.Slave/WebScrapper.Slave.csproj

# Copy source
COPY WebScrapper.Shared/ WebScrapper.Shared/
COPY WebScrapper.Slave/ WebScrapper.Slave/

# Build the project
RUN dotnet build WebScrapper.Slave/WebScrapper.Slave.csproj -c Release

# Install Playwright browsers and system dependencies
ENV PLAYWRIGHT_BROWSERS_PATH=/ms-playwright
RUN pwsh WebScrapper.Slave/bin/Release/net8.0/playwright.ps1 install --with-deps chromium

# Publish the .NET function app
RUN dotnet publish WebScrapper.Slave/WebScrapper.Slave.csproj -c Release -o /home/site/wwwroot

# Stage 2: Final runtime image with necessary dependencies for Playwright
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0

# Set environment variables for Azure Functions and Playwright
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    PLAYWRIGHT_BROWSERS_PATH=/ms-playwright

# Install system dependencies required by Playwright (Chromium)
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

# Copy Playwright browsers from build stage
COPY --from=build-env /ms-playwright /ms-playwright
RUN chmod -R 755 /ms-playwright

# Copy the published .NET function app from the build stage
COPY --from=build-env /home/site/wwwroot /home/site/wwwroot
