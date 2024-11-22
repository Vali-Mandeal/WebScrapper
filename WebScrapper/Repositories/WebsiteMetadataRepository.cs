using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using WebScrapper.Entities;
using WebScrapper.Repositories.Interfaces;

namespace WebScrapper.Repositories;
internal class WebsiteMetadataRepository : IWebsiteMetadataRepository
{
    private readonly ILogger _logger;

    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<WebsiteMetadata> _collection;

    public WebsiteMetadataRepository(ILogger<WebsiteMetadataRepository> logger, IMongoDatabase database)
    {
        _logger = logger;
        _database = database;
        _collection = _database.GetCollection<WebsiteMetadata>("WebsiteMetadatas");
    }

    public async Task AddAsync(WebsiteMetadata websiteMetadata)
    {
        try
        {
            await _collection.InsertOneAsync(websiteMetadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
    }

    public async Task<WebsiteMetadata> GetAsync(int id)
    {
        var filter = Builders<WebsiteMetadata>.Filter.Eq(x => x.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }
}
