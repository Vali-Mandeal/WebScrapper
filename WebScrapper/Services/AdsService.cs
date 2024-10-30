using WebScrapper.Entities;
using WebScrapper.Repositories.Interfaces;
using WebScrapper.Services.Extensions;
using WebScrapper.Services.Interfaces;

namespace WebScrapper.Services;
public class AdsService : IAdsService
{
    private readonly IAdsRepository _adsRepository;

    public AdsService(IAdsRepository adsRepository)
    {
        _adsRepository = adsRepository;
    }

    public async Task<List<Ad>> GetNewAsync(List<Ad> scrappedAds, ScrapJob scrapJob)
    {
        var existingsAds = await _adsRepository.GetByFilterAsync(propertyName: "ScrapJobId", filter: scrapJob.Id.ToString());

        SetIdsForCurrentAds(scrappedAds, scrapJob);

        var newAds = scrappedAds
            .Where(x => !existingsAds.Any(y => y.Id == x.Id))
            .ToList();

        var distinctNewAds = newAds
            .DistinctBy(x => x.Id)
            .ToList();

        SetAdsMetadata(distinctNewAds, scrapJob);

        return distinctNewAds;
    }

    public async Task AddAsync(List<Ad> ads)
    {
        try
        {
            await _adsRepository.CreateAsync(ads);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private static List<Ad> SetAdsMetadata(List<Ad> newAds, ScrapJob scrapJob)
    {
        foreach (Ad ad in newAds)
        {
            ad.ShouldSendNotification = ShouldSendNotification(ad, scrapJob);
        }

        return newAds;
    }

    private static bool ShouldSendNotification(Ad ad, ScrapJob scrapJob)
    {
        if (!ContainsRequiredKeywords(ad.Title, scrapJob.MustContainList))
            return false;

        if (ad.Price.GetPrice() > scrapJob.MaxPrice)
            return false;

        if (ContainsExcludedKeywords(ad.Title, scrapJob.MustNotContainList))
            return false;

        return true;
    }

    private static bool ContainsRequiredKeywords(string title, List<string> mustContainList)
    {
        return mustContainList.Any(item => title.Contains(item, StringComparison.InvariantCultureIgnoreCase));
    }

    private static bool ContainsExcludedKeywords(string title, List<string> mustNotContainList)
    {
        return mustNotContainList.Any(item => title.Contains(item, StringComparison.InvariantCultureIgnoreCase));
    }

    private static void SetIdsForCurrentAds(List<Ad> currentAds, ScrapJob scrapJob)
    {
        foreach (var ad in currentAds)
        {
            if (ad.Url == null)
            {
                Console.WriteLine($"Ad url is null. Ad: Id={ad.Id}, Title={ad.Title}, Price={ad.Price}, Location={ad.LocationAndDate}, Scrajobid: {ad.ScrapJobId}");
                continue;
            }

            ad.Id = ad.Url.GetId();
            ad.ScrapJobId = scrapJob.Id;
        }
    }
}
