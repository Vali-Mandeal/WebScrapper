using WebScrapper.Entities;
using WebScrapper.Repositories.Interfaces;
using WebScrapper.Services.Interfaces;

namespace WebScrapper.Services;
public class ScrapJobsService : IScrapJobsService
{
    private readonly IScrapJobsRepository _scrapJobsRepository;

    public ScrapJobsService(IScrapJobsRepository scrapJobsRepository)
    {
        _scrapJobsRepository = scrapJobsRepository;
    }

    public async Task<List<ScrapJob>> GetScrapJobsAsync()
    {
        return await _scrapJobsRepository.GetAllScrapJobsAsync();
    }

    public async Task AddScrapJobAsync(ScrapJob scrapJob)
    {
        await _scrapJobsRepository.AddAsync(scrapJob);
    }
}
