using WebScrapper.Entities;

namespace WebScrapper.Services.Interfaces;
public interface IScrapService
{
    Task<List<Ad>> GetCurrentAdsFromWebsiteAsync(ScrapJob scrapJob);
}
