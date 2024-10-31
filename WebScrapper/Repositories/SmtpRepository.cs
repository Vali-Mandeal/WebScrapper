using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MimeKit;

using Polly.Registry;

using WebScrapper.Entities;
using WebScrapper.Factories.Interfaces;
using WebScrapper.Repositories.Interfaces;

using static WebScrapper.Common.WebScrapperConstants;

namespace WebScrapper.Repositories;

public class SmtpRepository : INotificationRepository
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ISmtpClientFactory _smtpClientFactory;
    private readonly ResiliencePipelineProvider<string> _resiliencePipelineProvider;
    private readonly ILogger _logger;

    public SmtpRepository(IOptions<SmtpSettings> smtpSettings, ISmtpClientFactory smtpClientFactory, ResiliencePipelineProvider<string> resiliencePipelineProvider, ILogger logger)
    {
        _smtpSettings = smtpSettings.Value;
        _smtpClientFactory = smtpClientFactory;
        _resiliencePipelineProvider = resiliencePipelineProvider;
        _logger = logger;
    }


    public async Task SendNotificationAsync(Notification notification)
    {
        using var smtpClient = await _smtpClientFactory.CreateAsync();

        foreach (var receiver in notification.Receivers)
        {
            var message = new MimeMessage();
            SetEmailMetadata(notification, receiver, message);
            GetEmailBody(notification, message);

            var resiliencePipeline = _resiliencePipelineProvider.GetPipeline(SendEmailResiliencePipelineName);
            await resiliencePipeline.ExecuteAsync(async _ =>
            {
                try
                {
                    await smtpClient.SendAsync(message);
                    _logger.LogInformation($"Notification sent successfully to: {receiver.Email}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error sending notification to: {receiver.Email}, {ex.Message}");
                    throw;
                }
            });
        }
    }

    private void SetEmailMetadata(Notification notification, NotificationReceiver receiver, MimeMessage message)
    {
        message.From.Add(new MailboxAddress(notification.Job, _smtpSettings.SenderEmail));
        message.To.Add(new MailboxAddress(receiver.Name, receiver.Email));
        message.Subject = notification.Subject;
    }

    private static void GetEmailBody(Notification notification, MimeMessage message)
    {
        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = notification.Body
        };
        message.Body = bodyBuilder.ToMessageBody();
    }
}
