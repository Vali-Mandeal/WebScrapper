using Microsoft.Extensions.Logging;

using MimeKit;

using Polly;

using WebScrapper.Entities;
using WebScrapper.Factories.Interfaces;
using WebScrapper.Repositories.Interfaces;

namespace WebScrapper.Repositories;

public class SmtpRepository : INotificationRepository
{
    private readonly ISmtpClientFactory _smtpClientFactory;
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger _logger;

    public SmtpRepository(ISmtpClientFactory smtpClientFactory, SmtpSettings smtpSettings, ILogger logger)
    {
        this._smtpClientFactory = smtpClientFactory;
        this._smtpSettings = smtpSettings;
        this._logger = logger;
    }


    public async Task SendNotificationAsync(Notification notification)
    {
        using var smtpClient = await _smtpClientFactory.CreateAsync();

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
                if (!smtpClient.IsConnected)
                {
                    await ConnectAndAuthenticateSmtpClient();
                }

                try
                {
                    await smtpClient.SendAsync(message);
                    _logger.LogInformation($"Notification sent successfully to: {receiver.Email}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error sending notification to: {receiver.Email}, {ex.Message}");
                    throw;
                }
            });
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
}