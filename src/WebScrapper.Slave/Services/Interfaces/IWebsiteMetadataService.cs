using WebScrapper.Shared.Entities;

namespace WebScrapper.Slave.Services.Interfaces;
public interface IWebsiteMetadataService
{
    public Task<WebsiteMetadata> GetAsync(int id);
    public Task AddAsync(WebsiteMetadata websiteMetadata);
}
