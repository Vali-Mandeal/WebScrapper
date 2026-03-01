using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using WebScrapper.Shared.Configuration;
using WebScrapper.Shared.Repositories;
using WebScrapper.Shared.Repositories.Interfaces;
using WebScrapper.Shared.Services;
using WebScrapper.Shared.Services.Interfaces;

namespace WebScrapper.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DbSettings>(configuration.GetSection(DbSettings.Key));

        var dbSettings = configuration.GetSection(DbSettings.Key).Get<DbSettings>();

        services.AddSingleton<IMongoClient, MongoClient>(_ => new MongoClient(dbSettings!.MongoUrl));
        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(dbSettings!.DatabaseName);
        });

        services.AddScoped<IScrapJobsRepository, ScrapJobsRepository>();
        services.AddScoped<IScrapJobsService, ScrapJobsService>();

        return services;
    }
}
