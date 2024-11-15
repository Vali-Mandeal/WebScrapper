using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

using WebScrapper.Entities;
using WebScrapper.Services.Interfaces;

namespace WebScrapper.Services;
public class ScrapService : IScrapService
{
    private ILogger _logger;

    private const bool _headless = true;

    public ScrapService(ILogger<ScrapService> logger)
    {
        _logger = logger;
    }

    public async Task<List<Ad>> GetCurrentAdsFromWebsiteAsync(ScrapJob scrapJob)
    {
        IBrowser browser = await GetBrowserAsync();
        IPage page = await GetPageAsync(browser);

        await LoadPageAsync(page, scrapJob);
        await AcceptTermsAndConditionsAsync(page);
        await SearchAsync(page, scrapJob.SearchValue);
        await ScrollPageToBottomAsync(page);

        var cards = await GetCardAdsAsync(page);
        var ads = await ExtractAdsFromCardsAsync(scrapJob, cards);

        await browser.CloseAsync();

        return ads;
    }

    private static async Task<IBrowser> GetBrowserAsync()
    {
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = _headless });

        return browser;
    }

    private static async Task<IPage> GetPageAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        return page;
    }

    private static async Task LoadPageAsync(IPage page, ScrapJob scrapJob)
    {
        await page.GotoAsync(scrapJob.Url);
    }

    private async Task AcceptTermsAndConditionsAsync(IPage page)
    {
        try
        {
            var acceptButton = page.Locator("button[id=onetrust-accept-btn-handler]");
            await acceptButton.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
            await acceptButton.ClickAsync();
        }
        catch (TimeoutException)
        {
            _logger.LogError("Catch button did not appear, moving on.");
        }
    }

    private static async Task ScrollPageToBottomAsync(IPage page)
    {
        for (int i = 0; i < 50; i++)
        {
            await page.EvaluateAsync("window.scrollBy(0, 200)");
            await Task.Delay(500);
        }
    }

    private static async Task<IReadOnlyList<IElementHandle>> GetCardAdsAsync(IPage page)
    {
        await Task.Delay(3000);

        await page.WaitForSelectorAsync("div[data-testid='l-card']");

        var cards = await page.QuerySelectorAllAsync("div[data-testid='l-card']");
        return cards;
    }

    private static async Task SearchAsync(IPage page, string searchValue)
    {
        var searchBox = page.Locator("input[placeholder='Ce anume cauți?']");
        await searchBox.WaitForAsync();
        await searchBox.FillAsync(searchValue);

        await searchBox.PressAsync("Enter");
    }

    private static async Task<List<Ad>> ExtractAdsFromCardsAsync(ScrapJob scrapJob, IReadOnlyList<IElementHandle> cards)
    {
        var ads = new List<Ad>();

        foreach (var card in cards)
        {
            Ad ad = await GetAdAsync(card, scrapJob);

            ads.Add(ad);
        }

        return ads;
    }

    private static async Task<Ad> GetAdAsync(IElementHandle card, ScrapJob scrapJob)
    {
        var ad = new Ad();

        var titleElement = await card.QuerySelectorAsync("h6");
        ad.Title = titleElement != null ? await titleElement.InnerTextAsync() : null;

        var priceElement = await card.QuerySelectorAsync("p[data-testid='ad-price']");
        ad.Price = priceElement != null ? await priceElement.InnerTextAsync() : null;

        var locationDateElement = await card.QuerySelectorAsync("p[data-testid='location-date']");
        ad.LocationAndDate = locationDateElement != null ? await locationDateElement.InnerTextAsync() : null;

        var adUrlElement = await card.QuerySelectorAsync("a.css-qo0cxu");
        var partialUrl = adUrlElement != null ? await adUrlElement.GetAttributeAsync("href") : null;
        ad.Url = partialUrl != null ? scrapJob.Url + partialUrl : null;

        var thumbnailElement = await card.QuerySelectorAsync("img[src]");
        ad.ThumbnailUrl = thumbnailElement != null ? await thumbnailElement.GetAttributeAsync("src") : null;

        // Url fallback
        if (ad.ThumbnailUrl == null)
        {
            thumbnailElement = await card.QuerySelectorAsync("img[data-src]");
            ad.ThumbnailUrl = thumbnailElement != null ? await thumbnailElement.GetAttributeAsync("data-src") : null;
        }

        return ad;
    }
}
