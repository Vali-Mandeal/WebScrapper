using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WebScrapper.Shared.Entities;
using WebScrapper.Shared.Services.Interfaces;
using WebScrapper.Slave.Services.Interfaces;

namespace WebScrapper.Slave;

public class SlaveFunction
{
    private readonly ILogger _logger;
    private readonly IScrapJobsService _scrapJobsService;
    private readonly IScrapService _scrapService;
    private readonly IAdsService _adsService;
    private readonly INotificationService _notificationService;
    private readonly IWebsiteMetadataService _websiteMetadataService;

    public SlaveFunction(
        ILogger<SlaveFunction> logger,
        IScrapJobsService scrapJobsService,
        IScrapService scrapService,
        IAdsService adsService,
        INotificationService notificationService,
        IWebsiteMetadataService websiteMetadataService)
    {
        _logger = logger;
        _scrapJobsService = scrapJobsService;
        _scrapService = scrapService;
        _adsService = adsService;
        _notificationService = notificationService;
        _websiteMetadataService = websiteMetadataService;
    }

    [Function("SlaveFunction")]
    public async Task Run([QueueTrigger("scrap-jobs-queue")] ScrapJobQueueMessage message)
    {
        _logger.LogInformation("Processing job: {Name} (ID: {Id})", message.ScrapJobName, message.ScrapJobId);

        var scrapJob = await _scrapJobsService.GetByIdAsync(message.ScrapJobId);
        var websiteMetadata = await _websiteMetadataService.GetAsync(scrapJob.WebsiteMetadataId);
        var currentAds = await _scrapService.GetCurrentAdsFromWebsiteAsync(scrapJob, websiteMetadata);
        var newAds = await _adsService.GetNewAsync(currentAds, scrapJob);

        if (!newAds.Any())
        {
            _logger.LogInformation("No new ads found for job {Name}", scrapJob.Name);
            return;
        }

        await _adsService.AddAsync(newAds);

        var notificationWorthyAds = newAds
            .Where(x => x.ShouldSendNotification)
            .ToList();

        await _notificationService.SendNotificationAsync(notificationWorthyAds, scrapJob);

        _logger.LogInformation("Job {Name} finished at: {Now}", scrapJob.Name, DateTime.UtcNow);
    }
}
