using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebScrapper.Shared.Extensions;
using WebScrapper.Slave.Configuration;
using WebScrapper.Slave.Repositories;
using WebScrapper.Slave.Repositories.Interfaces;
using WebScrapper.Slave.Services;
using WebScrapper.Slave.Services.Interfaces;

var host = new HostBuilder()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();

        var builtConfig = config.Build();
        var kvUrl = builtConfig["KeyVaultConfig:Url"];
        if (!string.IsNullOrEmpty(kvUrl))
            config.AddAzureKeyVault(new Uri(kvUrl), new DefaultAzureCredential(), new KeyVaultSecretManager());
    })
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.AddSharedServices(configuration);

        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.Key));

        services.AddScoped<IAdsRepository, AdsRepository>();
        services.AddScoped<INotificationRepository, SmtpRepository>();
        services.AddScoped<IWebsiteMetadataRepository, WebsiteMetadataRepository>();

        services.AddScoped<IAdsService, AdsService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IScrapService, ScrapService>();
        services.AddScoped<IWebsiteMetadataService, WebsiteMetadataService>();

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddLogging();
    })
    .Build();

host.Run();
