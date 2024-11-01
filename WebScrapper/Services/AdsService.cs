using Microsoft.Extensions.Logging;
using WebScrapper.Entities;
using WebScrapper.Repositories.Interfaces;
using WebScrapper.Services.Extensions;
using WebScrapper.Services.Interfaces;

namespace WebScrapper.Services;
public class AdsService : IAdsService
{
    private readonly ILogger _logger;
    private readonly IAdsRepository _adsRepository;

    public AdsService(ILogger<AdsService> logger, IAdsRepository adsRepository)
    {
        _logger = logger;
        _adsRepository = adsRepository;
    }

    public async Task<List<Ad>> GetNewAsync(List<Ad> scrappedAds, ScrapJob scrapJob)
    {
        var existingsAds = await _adsRepository.GetByFilterAsync(propertyName: "ScrapJobId", filter: scrapJob.Id.ToString());

        SetScrappedJobsIds(scrappedAds, scrapJob);

        var newAds = GetNewAds(scrappedAds, existingsAds);

        var genuineAds = GetGenuineAds(newAds, scrapJob);

        var newGenuineDistinctAds = genuineAds
            .DistinctBy(x => x.Id)
            .ToList();
                
        SetAdsMetadata(newGenuineDistinctAds, scrapJob);

        return newGenuineDistinctAds;
    }

    public async Task AddAsync(List<Ad> ads)
    {
        await _adsRepository.CreateAsync(ads);
    }

    private void SetScrappedJobsIds(List<Ad> currentAds, ScrapJob scrapJob)
    {
        foreach (var ad in currentAds)
        {
            if (ad.Url == null)
            {
                _logger.LogError($"Ad url is null. Ad: Id={ad.Id}, Title={ad.Title}, Price={ad.Price}, Location={ad.LocationAndDate}, Scrajobid: {ad.ScrapJobId}");
                continue;
            }

            ad.Id = ad.Url.GetId();
            ad.ScrapJobId = scrapJob.Id;
        }
    }

    private static List<Ad> GetNewAds(List<Ad> scrappedAds, List<Ad> existingsAds)
    {
        return scrappedAds
            .Where(x => !existingsAds.Any(y => y.Id == x.Id))
            .ToList();
    }

    private static List<Ad> GetGenuineAds(List<Ad> newAds, ScrapJob scrapJob)
    {
        return newAds
            .Where(newAd => IsAdGenuine(newAd, scrapJob))
            .ToList();
    }

    private static bool IsAdGenuine(Ad newAd, ScrapJob scrapJob)
    {
        return ContainsRequiredKeywords(newAd.Title, scrapJob.MustContainList)
            && !ContainsExcludedKeywords(newAd.Title, scrapJob.MustNotContainList);
    }

    private static List<Ad> SetAdsMetadata(List<Ad> newAds, ScrapJob scrapJob)
    {
        foreach (Ad ad in newAds)
            ad.ShouldSendNotification = ShouldSendNotification(ad, scrapJob);

        return newAds;
    }

    private static bool ShouldSendNotification(Ad ad, ScrapJob scrapJob)
    {
        if (ad.Price.GetPrice() > scrapJob.MaxPrice)
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
}
