using MongoDB.Driver;
using WebScrapper.Entities;
using WebScrapper.Repositories.Interfaces;

namespace WebScrapper.Repositories;
public class AdsRepository : IAdsRepository
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<Ad> _collection;

    public AdsRepository(IMongoDatabase database)
    {
        _database = database;
        _collection = _database.GetCollection<Ad>("Ads");
    }

    public async Task<List<Ad>> GetByFilterAsync(string propertyName, string? filter = null)
    {
        try
        {
            var filterDefinition = filter != null ? Builders<Ad>.Filter.Eq(propertyName, filter) : Builders<Ad>.Filter.Empty;
            var result = await _collection.Find(filterDefinition).ToListAsync();
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return new List<Ad>();
        }
    }

    public async Task CreateAsync(List<Ad> ads)
    {
        try
        {
            await _collection.InsertManyAsync(ads);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
