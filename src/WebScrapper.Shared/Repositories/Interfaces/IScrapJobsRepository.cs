using WebScrapper.Shared.Entities;

namespace WebScrapper.Shared.Repositories.Interfaces;
public interface IScrapJobsRepository
{
    Task<List<ScrapJob>> GetAsync();
    Task<ScrapJob> GetByIdAsync(int id);
    Task AddAsync(ScrapJob scrapJob);
}
