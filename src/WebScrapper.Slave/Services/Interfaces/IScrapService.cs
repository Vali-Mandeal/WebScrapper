using WebScrapper.Shared.Entities;

namespace WebScrapper.Slave.Services.Interfaces;
public interface IScrapService
{
    Task<List<Ad>> GetCurrentAdsFromWebsiteAsync(ScrapJob scrapJob, WebsiteMetadata websiteMetadata);
}
