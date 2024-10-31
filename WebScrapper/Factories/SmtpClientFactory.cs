using MailKit.Net.Smtp;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

    public async Task<SmtpClient> CreateAsync()
    {
        var smtpClient = new SmtpClient();
        await ConnectAsync(smtpClient);
        await AuthenticateAsync(smtpClient);

        return smtpClient;
    }

    private async Task ConnectAsync(SmtpClient smtpClient)
    {
        try
        {
            await smtpClient.ConnectAsync(_smtpSettings.SmtpHost, _smtpSettings.SmtpPort, _smtpSettings.SecureSocketOptions);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"Error connecting SMTP client: {exception.Message}");
            throw;
        }
    }
    private async Task AuthenticateAsync(SmtpClient smtpClient)
    {
        try
        {
            await smtpClient.AuthenticateAsync(_smtpSettings.SenderEmail, _smtpSettings.SenderPassword);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"Error authenticating SMTP client: {exception.Message}");
            throw;
        }
    }
}
