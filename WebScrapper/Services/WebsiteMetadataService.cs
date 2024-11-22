using WebScrapper.Entities;
using WebScrapper.Repositories.Interfaces;
using WebScrapper.Services.Interfaces;

namespace WebScrapper.Services;
public class WebsiteMetadataService : IWebsiteMetadataService
{
    private readonly IWebsiteMetadataRepository _websiteMetadataRepository;

    public WebsiteMetadataService(IWebsiteMetadataRepository websiteMetadataRepository)
    {
        _websiteMetadataRepository = websiteMetadataRepository;
    }

    public async Task AddAsync(WebsiteMetadata websiteMetadata)
    {
        await _websiteMetadataRepository.AddAsync(websiteMetadata);
    }

    public async Task<WebsiteMetadata> GetAsync(int id)
    {
        var websiteMetadata = await _websiteMetadataRepository.GetAsync(id);

        return websiteMetadata;
    }
}
