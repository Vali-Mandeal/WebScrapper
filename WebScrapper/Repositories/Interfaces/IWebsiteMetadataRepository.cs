using WebScrapper.Entities;

namespace WebScrapper.Repositories.Interfaces;
public interface IWebsiteMetadataRepository
{
    public Task<WebsiteMetadata> GetAsync(int id);
    public Task AddAsync(WebsiteMetadata websiteMetadata);
}
    