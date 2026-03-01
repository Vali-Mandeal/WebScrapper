using WebScrapper.Shared.Entities;

namespace WebScrapper.Slave.Repositories.Interfaces;
public interface IWebsiteMetadataRepository
{
    public Task<WebsiteMetadata> GetAsync(int id);
    public Task AddAsync(WebsiteMetadata websiteMetadata);
}
