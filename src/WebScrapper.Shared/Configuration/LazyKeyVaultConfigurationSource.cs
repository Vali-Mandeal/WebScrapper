using System.Collections.Concurrent;
using Azure.Core;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace WebScrapper.Shared.Configuration;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddLazyKeyVault(this IConfigurationBuilder builder, string kvUrl, TokenCredential credential)
    {
        if (!string.IsNullOrEmpty(kvUrl))
            builder.Add(new LazyKeyVaultConfigurationSource(new Uri(kvUrl), credential));

        return builder;
    }
}

internal class LazyKeyVaultConfigurationSource(Uri vaultUri, TokenCredential credential) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var options = new SecretClientOptions();
        options.Retry.MaxRetries = 0;
        options.Retry.NetworkTimeout = TimeSpan.FromSeconds(3);
        return new LazyKeyVaultConfigurationProvider(new SecretClient(vaultUri, credential, options));
    }
}

internal class LazyKeyVaultConfigurationProvider(SecretClient client) : ConfigurationProvider
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
        // KV secrets map to hierarchical config keys (e.g. DbSettings--MongoUrl → DbSettings:MongoUrl).
        // Flat keys like "urls", "http_ports" can never be in KV — skip them immediately.
        if (!key.Contains(':'))
            return NotFound;

        var secretName = key.Replace(":", "--");
        try
        {
            return client.GetSecret(secretName).Value.Value;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return NotFound;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[LazyKeyVault] Failed to fetch secret '{secretName}': {ex.GetType().Name}: {ex.Message}");
            return NotFound;
        }
    }
}
