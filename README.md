#Webscrapper

WebScrapper is a personal project designed to track deals on local stores. Whenever a deal is found, it sends email notifications to keep me updated on the latest bargains.

## Features

- **Timer Triggered Scraping**: Automatically scrape websites at scheduled intervals using Azure Functions Timer Triggers.
- **HTTP Triggered Scraping**: Manually trigger scraping via HTTP requests.
- **Playwright Integration**: Use Playwright to interact with and scrape dynamic web pages.
- **MongoDB Integration**: Store and manage scraped ads in a MongoDB database.
- **Email Notifications**: Send email notifications for new ads that match specified criteria.
- **Docker Support**: Easily deploy the application using Docker and Docker Compose.


## Getting Started

### Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- [MongoDB](https://www.mongodb.com/)

### Installation

1. **Clone the repository:**

    ```sh
    git clone https://github.com/yourusername/WebScrapper.git
    cd WebScrapper
    ```

2. **Build the Docker image:**

    ```sh
    docker-compose build
    ```

3. **Run the Docker container:**

    ```sh
    docker-compose up
    ```

### Configuration

- **MongoDB Connection String:** Update the `ConnectionStrings__MongoUrl` environment variable in `docker-compose.yml` with your MongoDB connection string.
- **SMTP Settings:** Update the SMTP settings in `docker-compose.yml` with your email provider's details.

### Deployment

1. **Deploy to Azure:**

    ```sh
    ./Infra/deploy.ps1
    ```

2. **Monitor the deployment:**

    Check the Azure portal for the status of your resources.

## Usage

### Timer Trigger

The function is set to run at specific intervals as defined in the cron expression in `Function1.cs`:

```cs
[Function("Function1")]
public async Task Run([TimerTrigger("0 7-20 * * *")] TimerInfo myTimer)
```

### HTTP Trigger
You can manually trigger the scraping process via an HTTP request:
```cs
[Function("Function1_HttpTrigger")]
public async Task<HttpResponseData> RunHttpTrigger([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
```

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Acknowledgements

- [Playwright](https://playwright.dev/)
- [Azure Functions](https://docs.microsoft.com/en-us/azure/azure-functions/)
- [MongoDB](https://www.mongodb.com/)
