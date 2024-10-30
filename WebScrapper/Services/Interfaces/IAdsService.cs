using WebScrapper.Entities;

namespace WebScrapper.Services.Interfaces;
public interface IAdsService
{
    /// <summary>
    /// Identifies the new ads by comparing the <paramref name="scrappedAds"/> against existing ads stored in the database
    /// </summary>
    /// <param name="scrappedAds"></param>
    /// <param name="scrapJob"></param>
    /// <returns>A list of ads from <paramref name="scrappedAds"/> that do not already exist in the database</returns>
    Task<List<Ad>> GetNewAsync(List<Ad> scrappedAds, ScrapJob scrapJob);

    Task AddAsync(List<Ad> ads);
}
