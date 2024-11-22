using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using WebScrapper.Entities;
using WebScrapper.Repositories.Interfaces;

namespace WebScrapper.Repositories;
public class AdsRepository : IAdsRepository
{
    private readonly ILogger _logger;
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<Ad> _collection;

    public AdsRepository(ILogger<AdsRepository> logger, IMongoDatabase database)
    {
        _logger = logger;
        _database = database;
        _collection = _database.GetCollection<Ad>("Ads");
    }

    public async Task<List<Ad>> GetAsync(string propertyName, string? filter = null)
    {
        try
        {
            var filterDefinition = filter != null ? Builders<Ad>.Filter.Eq(propertyName, filter) : Builders<Ad>.Filter.Empty;
            var result = await _collection.Find(filterDefinition).ToListAsync();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return [];
        }
    }

    public async Task AddAsync(List<Ad> ads)
    {
        try
        {
            await _collection.InsertManyAsync(ads);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
    }
}
