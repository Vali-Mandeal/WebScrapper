using WebScrapper.Entities;

namespace WebScrapper.Repositories.Interfaces;
public interface IScrapJobsRepository
{
    Task<List<ScrapJob>> GetAllScrapJobsAsync();
    Task AddAsync(ScrapJob scrapJob);
}
