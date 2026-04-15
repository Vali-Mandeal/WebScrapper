using System.Collections.Concurrent;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
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
        {
            var credential = new DefaultAzureCredential();
            config.Add(new LazyKeyVaultConfigurationSource(new Uri(kvUrl), credential));
        }
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

class LazyKeyVaultConfigurationSource(Uri vaultUri, TokenCredential credential) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
        => new LazyKeyVaultConfigurationProvider(new SecretClient(vaultUri, credential));
}

class LazyKeyVaultConfigurationProvider(SecretClient client) : ConfigurationProvider
{
    private const string NotFound = "\0";
    private readonly ConcurrentDictionary<string, Lazy<string>> _cache = new(StringComparer.OrdinalIgnoreCase);

    public override void Load() { }

    public override bool TryGet(string key, out string? value)
    {
        var result = _cache.GetOrAdd(key, k => new Lazy<string>(() => Fetch(k))).Value;
        if (result == NotFound)
        {
            value = null;
            return false;
        }
        value = result;
        return true;
    }

    private string Fetch(string key)
    {
        // .NET config uses ":" as separator; Key Vault secret names use "--"
        var secretName = key.Replace(":", "--");
        try
        {
            return client.GetSecret(secretName).Value.Value;
        }
        catch
        {
            return NotFound;
        }
    }
}
