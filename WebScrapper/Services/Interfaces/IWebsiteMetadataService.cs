using WebScrapper.Entities;

namespace WebScrapper.Services.Interfaces;
public interface IWebsiteMetadataService
{
    public Task<WebsiteMetadata> GetAsync(int id);
    public Task AddAsync(WebsiteMetadata websiteMetadata);
}
