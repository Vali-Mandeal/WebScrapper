using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using WebScrapper.Services.Interfaces;

namespace WebScrapper;

public class Function1
{
    private readonly ILogger _logger;
    private readonly IScrapJobsService _scrapJobsService;
    private readonly IScrapService _scrapService;
    private readonly IAdsService _adsService;
    private readonly INotificationService _notificationService;
    private readonly IWebsiteMetadataService _websiteMetadataService;

    public Function1(ILogger<Function1> logger, IScrapJobsService scrapJobsService, IScrapService scrapService, IAdsService adsService, INotificationService notificationService, IWebsiteMetadataService websiteMetadataService)
    {
        _logger = logger;
        _scrapJobsService = scrapJobsService;
        _scrapService = scrapService;
        _adsService = adsService;
        _notificationService = notificationService;
        _websiteMetadataService = websiteMetadataService;
    }

    [Function("Function1_HttpTrigger")]
    public async Task<HttpResponseData> RunHttpTrigger([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
    {
        await ExecuteScrapJobs();
        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        await response.WriteStringAsync("Function executed successfully.");
        return response;
    }

    [Function("Function1")]
    public async Task Run([TimerTrigger("0 7-20 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.UtcNow}");

        await ExecuteScrapJobs();

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
        }
    }

    private async Task ExecuteScrapJobs()
    {
        _logger.LogInformation($"C# function executed at: {DateTime.UtcNow}");

        //await InitializePlaywrightAsync();

        var scrapJobs = await _scrapJobsService.GetAsync();

        foreach (var scrapJob in scrapJobs)
        {
            _logger.LogInformation($"Job {scrapJob.Name} started at: {DateTime.UtcNow}");

            var websiteMetadata = await _websiteMetadataService.GetAsync(scrapJob.WebsiteMetadataId);
            var currentAds = await _scrapService.GetCurrentAdsFromWebsiteAsync(scrapJob, websiteMetadata);
            var newAds = await _adsService.GetNewAsync(currentAds, scrapJob);

            if (!newAds.Any())
            {
                _logger.LogInformation($"No new ads found for job {scrapJob.Name}");
                continue;
            }

            await _adsService.AddAsync(newAds);

            var notificationWorthyAds = newAds
                .Where(x => x.ShouldSendNotification)
                .ToList();

            await _notificationService.SendNotificationAsync(notificationWorthyAds, scrapJob);

            _logger.LogInformation($"Job {scrapJob.Name} finished at: {DateTime.UtcNow}");
        }
    }

    private async Task InitializePlaywrightAsync()
    {
        var exitCode = Microsoft.Playwright.Program.Main(["install", "chromium"]);
        if (exitCode != 0)
            _logger.LogError($"Playwright exited with code {exitCode}");
    }
}
