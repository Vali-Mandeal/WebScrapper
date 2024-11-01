using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MongoDB.Driver;

using Polly;
using Polly.Retry;

using WebScrapper.Factories;
using WebScrapper.Factories.Interfaces;
using WebScrapper.Repositories;
using WebScrapper.Repositories.Interfaces;
using WebScrapper.Services;
using WebScrapper.Services.Interfaces;

using static WebScrapper.Common.WebScrapperConstants;

var host = new HostBuilder()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddLogging();

        var mongoConnectionString = configuration.GetConnectionString("MongoUrl");
        services.AddSingleton<IMongoClient, MongoClient>(_ => new MongoClient(mongoConnectionString));

        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase("WebCrawlerDb");
        });

        services.AddOptions<SmtpSettings>()
            .Bind(configuration.GetSection(SmtpSettings.SectionName))
            .ValidateOnStart();

        services.AddSingleton<ISmtpClientFactory, SmtpClientFactory>();

        services.AddResiliencePipeline(SendEmailResiliencePipelineName, x =>
        {
            x.AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential
            });
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