using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using WebScrapper.Configuration;
using WebScrapper.Repositories;
using WebScrapper.Repositories.Interfaces;
using WebScrapper.Services;
using WebScrapper.Services.Interfaces;

var host = new HostBuilder()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();

        var builtConfig = config.Build();

        var credential = new DefaultAzureCredential();
        config.AddAzureKeyVault(new Uri(builtConfig["KeyVaultConfig:Url"]), credential, new KeyVaultSecretManager());
    })
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.Key));
        services.Configure<DbSettings>(configuration.GetSection(DbSettings.Key));

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddLogging();

        var dbSettings = configuration.GetSection(DbSettings.Key).Get<DbSettings>();
        var smtpSettings = configuration.GetSection(SmtpSettings.Key).Get<SmtpSettings>();

        services.AddSingleton<IMongoClient, MongoClient>(_ => new MongoClient(dbSettings.MongoUrl));

        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(dbSettings.DatabaseName);
        });

        services.AddScoped<IAdsRepository, AdsRepository>();
        services.AddScoped<IScrapJobsRepository, ScrapJobsRepository>();
        services.AddScoped<INotificationRepository, SmtpRepository>();

        services.AddScoped<IAdsService, AdsService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IScrapJobsService, ScrapJobsService>();
        services.AddScoped<IScrapService, ScrapService>();

    })
    .Build();

host.Run();