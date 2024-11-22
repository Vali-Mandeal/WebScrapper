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
        _logger.LogInformation($"Processing new ads.");

        var existingsAds = await _adsRepository.GetAsync(propertyName: "ScrapJobId", filter: scrapJob.Id.ToString());

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
        _logger.LogInformation($"Saving new ads. Count: {ads.Count}");

        await _adsRepository.AddAsync(ads);
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
        var newAds = scrappedAds
            .Where(x => !existingsAds.Any(y => y.Id == x.Id))
            .ToList();

        return newAds;
    }

    private static List<Ad> GetGenuineAds(List<Ad> newAds, ScrapJob scrapJob)
    {
        var genuineAds = newAds
            .Where(newAd => IsAdGenuine(newAd, scrapJob))
            .ToList();

        return genuineAds;
    }

    private static bool IsAdGenuine(Ad newAd, ScrapJob scrapJob)
    {
        var isAdGenuine = ContainsRequiredKeywords(newAd.Title, scrapJob.MustContainList)
                      && !ContainsExcludedKeywords(newAd.Title, scrapJob.MustNotContainList);

        return isAdGenuine;
    }

    private static List<Ad> SetAdsMetadata(List<Ad> newAds, ScrapJob scrapJob)
    {
        foreach (Ad ad in newAds)
            ad.ShouldSendNotification = ShouldSendNotification(ad, scrapJob);

        return newAds;
    }

    private static bool ShouldSendNotification(Ad ad, ScrapJob scrapJob)
    {
        if (ad.Price?.GetPrice() > scrapJob.MaxPrice)
            return false;

        return true;
    }

    private static bool ContainsRequiredKeywords(string title, List<string> mustContainList)
    {
        var containsRequiredkeyWords = mustContainList.All(item => title.Contains(item, StringComparison.InvariantCultureIgnoreCase));

        return containsRequiredkeyWords;
    }

    private static bool ContainsExcludedKeywords(string title, List<string> mustNotContainList)
    {
        var containsExcludedKeywords = mustNotContainList.Any(item => title.Contains(item, StringComparison.InvariantCultureIgnoreCase));

        return containsExcludedKeywords;
    }
}
