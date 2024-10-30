using WebScrapper.Entities;

namespace WebScrapper.Services.Interfaces;
public interface IScrapJobsService
{
    Task<List<ScrapJob>> GetScrapJobsAsync();
    Task AddScrapJobAsync(ScrapJob scrapJob);
}
