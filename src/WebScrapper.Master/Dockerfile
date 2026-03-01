# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

WORKDIR /src

# Copy project files and restore
COPY WebScrapper.Shared/WebScrapper.Shared.csproj WebScrapper.Shared/
COPY WebScrapper.Master/WebScrapper.Master.csproj WebScrapper.Master/
RUN dotnet restore WebScrapper.Master/WebScrapper.Master.csproj

# Copy source and publish
COPY WebScrapper.Shared/ WebScrapper.Shared/
COPY WebScrapper.Master/ WebScrapper.Master/
RUN dotnet publish WebScrapper.Master/WebScrapper.Master.csproj -c Release -o /home/site/wwwroot

# Stage 2: Runtime
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=build-env /home/site/wwwroot /home/site/wwwroot
