using WebScrapper.Entities;

namespace WebScrapper.Repositories.Interfaces;
public interface IScrapJobsRepository
{
    Task<List<ScrapJob>> GetAsync();
    Task AddAsync(ScrapJob scrapJob);
}
