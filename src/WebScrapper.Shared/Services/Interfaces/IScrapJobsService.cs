using WebScrapper.Shared.Entities;

namespace WebScrapper.Shared.Services.Interfaces;
public interface IScrapJobsService
{
    Task<List<ScrapJob>> GetAsync();
    Task<ScrapJob> GetByIdAsync(int id);
    Task AddAsync(ScrapJob scrapJob);
}
