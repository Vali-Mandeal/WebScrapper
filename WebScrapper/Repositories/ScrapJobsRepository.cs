using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using WebScrapper.Entities;
using WebScrapper.Repositories.Interfaces;

namespace WebScrapper.Repositories;
public class ScrapJobsRepository : IScrapJobsRepository
{
    private readonly ILogger _logger;

    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<ScrapJob> _collection;

    public ScrapJobsRepository(ILogger<ScrapJobsRepository> logger, IMongoDatabase database)
    {
        _logger = logger;
        _database = database;
        _collection = _database.GetCollection<ScrapJob>("ScrapJobs");
    }

    public async Task<List<ScrapJob>> GetAsync()
    {
        try
        {
            var filter = Builders<ScrapJob>.Filter.Empty;
            var result = await _collection.Find(filter).ToListAsync();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return [];
        }
    }

    public async Task AddAsync(ScrapJob scrapJob)
    {
        try
        {
            await _collection.InsertOneAsync(scrapJob);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
    }
}
