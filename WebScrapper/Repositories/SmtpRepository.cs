using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using WebScrapper.Entities;
using WebScrapper.Repositories.Interfaces;
using Polly; 

namespace WebScrapper.Repositories;

public class SmtpRepository : INotificationRepository, IDisposable
{
    private readonly ILogger _logger;
    private readonly MailKit.Net.Smtp.SmtpClient _smtpClient;
    private readonly SmtpSettings _smtpSettings;

    public SmtpRepository(ILogger<SmtpRepository> logger, IOptions<SmtpSettings> smtpSettings)
    {
        _logger = logger;
        _smtpClient = new MailKit.Net.Smtp.SmtpClient();
        _smtpSettings = smtpSettings.Value;
        ConnectAndAuthenticateSmtpClient().GetAwaiter().GetResult();
    }

    public async Task SendNotificationAsync(Notification notification)
    {
        foreach (var receiver in notification.Receivers)
        {
            var message = new MimeMessage();
            SetEmailMetadata(notification, receiver, message);
            GetEmailBody(notification, message);

            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning($"Retry {retryCount} for {receiver.Email} due to {exception.Message}");
                });

            await retryPolicy.ExecuteAsync(async () =>
            {
                if (!_smtpClient.IsConnected)
                {
                    await ConnectAndAuthenticateSmtpClient();
                }

                try
                {
                    await _smtpClient.SendAsync(message);
                    _logger.LogInformation($"Notification sent successfully to: {receiver.Email}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error sending notification to: {receiver.Email}, {ex.Message}");
                }
            });
        }

        Dispose();
    }

    private async Task ConnectAndAuthenticateSmtpClient()
    {
        try
        {
            await _smtpClient.ConnectAsync(_smtpSettings.SmtpHost, _smtpSettings.SmtpPort, _smtpSettings.SecureSocketOptions);
            await _smtpClient.AuthenticateAsync(_smtpSettings.SenderEmail, _smtpSettings.SenderPassword);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error connecting/authenticating SMTP client: {ex.Message}");
        }
    }

    private static void GetEmailBody(Notification notification, MimeMessage message)
    {
        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = notification.Body
        };
        message.Body = bodyBuilder.ToMessageBody();
    }

    private void SetEmailMetadata(Notification notification, NotificationReceiver receiver, MimeMessage message)
    {
        message.From.Add(new MailboxAddress(notification.Job, _smtpSettings.SenderEmail));
        message.To.Add(new MailboxAddress(receiver.Name, receiver.Email));
        message.Subject = notification.Subject;
    }

    public void Dispose()
    {
        if (_smtpClient.IsConnected)
        {
            _smtpClient.Disconnect(true);
        }
        _smtpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}