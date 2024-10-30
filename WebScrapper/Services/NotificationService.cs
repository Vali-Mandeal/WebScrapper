using Microsoft.Extensions.Logging;
using System.Text;
using WebScrapper.Entities;
using WebScrapper.Repositories.Interfaces;
using WebScrapper.Services.Interfaces;

namespace WebScrapper.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger _logger;
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(ILogger<NotificationService> logger, INotificationRepository notificationRepository)
    {
        _logger = logger;
        _notificationRepository = notificationRepository;
    }

    public async Task SendNotificationAsync(List<Ad> ads, ScrapJob scrapJob)
    {
        if (ads.Any() == false)
        {
            _logger.LogInformation($"No new ads for {scrapJob.Name}");
            return;
        }

        _logger.LogInformation($"Generating notification for job: {scrapJob.Name}, found {ads.Count} ads.");


        var htmlBody = GetHtmlBody(ads);

        var notification = new Notification(
             Receivers: scrapJob.NotificationReceivers,
             Subject: $"New ads for {scrapJob.Name} {DateTime.UtcNow}",
             Body: htmlBody,
             Job: scrapJob.Name
        );

        await _notificationRepository.SendNotificationAsync(notification);
    }

    private static string GetHtmlBody(List<Ad> ads)
    {
        var adsText = new StringBuilder();
        adsText.AppendLine("<html><body>");

        foreach (var ad in ads)
        {
            adsText.AppendLine($"<p><strong>{ad.Title}</strong></p>");
            adsText.AppendLine($"<p><strong>Price:</strong> {ad.Price}</p>");
            adsText.AppendLine($"<p><strong>Location and Date:</strong> {ad.LocationAndDate}</p>");
            adsText.AppendLine($"<p><strong>Link:</strong> <a href='{ad.Url}'>{ad.Url}</a></p>");
            adsText.AppendLine($"<p><img src='{ad.ThumbnailUrl}' alt='Thumbnail' /></p>");
            adsText.AppendLine("<hr>");
        }

        adsText.AppendLine("</body></html>");

        return adsText.ToString();
    }
}
