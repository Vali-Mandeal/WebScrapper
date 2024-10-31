using MailKit.Net.Smtp;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using WebScrapper.Adapters;
using WebScrapper.Factories.Interfaces;

namespace WebScrapper.Factories;

public class SmtpClientFactory : ISmtpClientFactory
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<SmtpClientFactory> _logger;

    public SmtpClientFactory(IOptions<SmtpSettings> smtpSettings, ILogger<SmtpClientFactory> logger)
    {
        _smtpSettings = smtpSettings.Value;
        _logger = logger;
    }


    public async Task<SmtpClientAdapter> CreateAsync()
    {
        var smtpClient = new SmtpClientAdapter(new SmtpClient(), _smtpSettings);
        await ConnectAsync(smtpClient);
        await AuthenticateAsync(smtpClient);

        return smtpClient;
    }

    private async Task ConnectAsync(SmtpClientAdapter smtpClient)
    {
        try
        {
            await smtpClient.ConnectAsync();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"Error connecting SMTP client: {exception.Message}");
            throw;
        }
    }
    private async Task AuthenticateAsync(SmtpClientAdapter smtpClient)
    {
        try
        {
            await smtpClient.AuthenticateAsync();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"Error authenticating SMTP client: {exception.Message}");
            throw;
        }
    }
}
