using MongoDB.Driver;
using WebScrapper.Entities;
using WebScrapper.Repositories.Interfaces;

namespace WebScrapper.Repositories;
public class ScrapJobsRepository : IScrapJobsRepository
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<ScrapJob> _collection;

    public ScrapJobsRepository(IMongoDatabase database)
    {

        _database = database;
        _collection = _database.GetCollection<ScrapJob>("ScrapJobs");
    }

    public async Task<List<ScrapJob>> GetAllScrapJobsAsync()
    {
        try
        {
            var filter = Builders<ScrapJob>.Filter.Empty;
            var result = await _collection.Find(filter).ToListAsync();
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return new List<ScrapJob>();
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
            Console.WriteLine(ex);
        }
    }
}
