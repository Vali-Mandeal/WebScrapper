using WebScrapper.Entities;

namespace WebScrapper.Services.Interfaces;
public interface IScrapJobsService
{
    Task<List<ScrapJob>> GetAsync();
    Task AddAsync(ScrapJob scrapJob);
}
