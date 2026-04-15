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
        => new LazyKeyVaultConfigurationProvider(new SecretClient(vaultUri, credential));
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
