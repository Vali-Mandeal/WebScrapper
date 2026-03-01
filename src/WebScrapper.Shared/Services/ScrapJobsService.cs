using WebScrapper.Shared.Entities;
using WebScrapper.Shared.Repositories.Interfaces;
using WebScrapper.Shared.Services.Interfaces;

namespace WebScrapper.Shared.Services;
public class ScrapJobsService : IScrapJobsService
{
    private readonly IScrapJobsRepository _scrapJobsRepository;

    public ScrapJobsService(IScrapJobsRepository scrapJobsRepository)
    {
        _scrapJobsRepository = scrapJobsRepository;
    }

    public async Task<List<ScrapJob>> GetAsync()
    {
        return await _scrapJobsRepository.GetAsync();
    }

    public async Task<ScrapJob> GetByIdAsync(int id)
    {
        return await _scrapJobsRepository.GetByIdAsync(id);
    }

    public async Task AddAsync(ScrapJob scrapJob)
    {
        await _scrapJobsRepository.AddAsync(scrapJob);
    }
}
