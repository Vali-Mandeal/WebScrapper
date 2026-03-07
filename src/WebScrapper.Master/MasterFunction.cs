using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WebScrapper.Shared.Entities;
using WebScrapper.Shared.Services.Interfaces;

namespace WebScrapper.Master;

public class MasterFunction
{
    private readonly ILogger _logger;
    private readonly IScrapJobsService _scrapJobsService;

    public MasterFunction(ILogger<MasterFunction> logger, IScrapJobsService scrapJobsService)
    {
        _logger = logger;
        _scrapJobsService = scrapJobsService;
    }

    [Function("MasterFunction_HttpTrigger")]
    public async Task<HttpTriggerOutput> RunHttpTrigger(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("HTTP trigger started at: {Now}", DateTime.UtcNow);

        var messages = await EnqueueScrapJobs();

        _logger.LogInformation("HTTP trigger completed. Enqueued {Count} scrap jobs.", messages.Count);

        return new HttpTriggerOutput
        {
            HttpResponse = TypedResults.Ok($"Enqueued {messages.Count} scrap jobs."),
            QueueMessages = messages
        };
    }

    [Function("MasterFunction")]
    [QueueOutput("scrap-jobs-queue")]
    public async Task<List<ScrapJobQueueMessage>> Run(
        [TimerTrigger("0 7-20 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("Timer trigger executed at: {Now}", DateTime.UtcNow);

        var messages = await EnqueueScrapJobs();

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {Next}", myTimer.ScheduleStatus.Next);
        }

        return messages;
    }

    private async Task<List<ScrapJobQueueMessage>> EnqueueScrapJobs()
    {
        var scrapJobs = await _scrapJobsService.GetAsync();

        _logger.LogInformation("Found {Count} scrap jobs, enqueueing.", scrapJobs.Count);

        var messages = scrapJobs.Select(job => new ScrapJobQueueMessage
        {
            ScrapJobId = job.Id,
            ScrapJobName = job.Name
        }).ToList();

        return messages;
    }
}

public class HttpTriggerOutput
{
    [QueueOutput("scrap-jobs-queue")]
    public List<ScrapJobQueueMessage> QueueMessages { get; set; } = [];

    [HttpResult]
    public IResult HttpResponse { get; set; } = null!;
}
