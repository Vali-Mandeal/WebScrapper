using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using WebScrapper.Shared.Entities;
using WebScrapper.Slave.Repositories.Interfaces;

namespace WebScrapper.Slave.Repositories;
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
            return await _collection.Find(filterDefinition).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB operation failed: {Message}", ex.Message);
            throw;
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
            _logger.LogError(ex, "MongoDB operation failed: {Message}", ex.Message);
            throw;
        }
    }
}
