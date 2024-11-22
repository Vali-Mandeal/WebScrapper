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

    public async Task<List<Ad>> GetCurrentAdsFromWebsiteAsync(ScrapJob scrapJob, WebsiteMetadata websiteMetadata)
    {
        IBrowser browser = await GetBrowserAsync();
        IPage page = await GetPageAsync(browser);

        await LoadPageAsync(page, scrapJob, websiteMetadata);
        await AcceptTermsAndConditionsAsync(page, websiteMetadata);
        await SearchAsync(page, scrapJob.SearchValue, websiteMetadata);
        await ScrollPageToBottomAsync(page, websiteMetadata);

        var cards = await GetCardAdsAsync(page, websiteMetadata);
        var ads = await ExtractAdsFromCardsAsync(scrapJob, cards, websiteMetadata);

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

    private static async Task LoadPageAsync(IPage page, ScrapJob scrapJob, WebsiteMetadata websiteMetadata)
    {
        await page.GotoAsync(websiteMetadata.Url);
    }

    private async Task AcceptTermsAndConditionsAsync(IPage page, WebsiteMetadata websiteMetadata)
    {
        if (websiteMetadata.ShouldAcceptTermsAndConditions is false)
            return;

        try
        {
            var acceptButton = page.Locator(websiteMetadata.Selectors.TermsAndConditionsButtonSelector);
            await acceptButton.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
            await acceptButton.ClickAsync();
        }
        catch (TimeoutException)
        {
            _logger.LogError("Catch button did not appear, moving on.");
        }
    }

    private static async Task SearchAsync(IPage page, string searchValue, WebsiteMetadata websiteMetadata)
    {
        if (websiteMetadata.ShouldSearch is false)
            return;

        var searchBox = page.Locator(websiteMetadata.Selectors.SearchSelector);
        await searchBox.WaitForAsync();
        await searchBox.FillAsync(searchValue);

        await searchBox.PressAsync("Enter");
    }

    private static async Task ScrollPageToBottomAsync(IPage page, WebsiteMetadata websiteMetadata)
    {
        if (websiteMetadata.ShouldScrollToBottom is false)
            return;

        for (int i = 0; i < 50; i++)
        {
            await page.EvaluateAsync(websiteMetadata.Selectors.ScrollToButtonCommand);
            await Task.Delay(500);
        }
    }

    private static async Task<IReadOnlyList<IElementHandle>> GetCardAdsAsync(IPage page, WebsiteMetadata websiteMetadata)
    {
        var cardSelector = websiteMetadata.Selectors.CardsSelector;

        await Task.Delay(3000);

        await page.WaitForSelectorAsync(cardSelector);

        var cards = await page.QuerySelectorAllAsync(cardSelector);

        return cards;
    }

  

    private static async Task<List<Ad>> ExtractAdsFromCardsAsync(ScrapJob scrapJob, IReadOnlyList<IElementHandle> cards, WebsiteMetadata websiteMetadata)
    {
        var ads = new List<Ad>();

        foreach (var card in cards)
        {
            Ad ad = await GetAdAsync(card, scrapJob, websiteMetadata);

            ads.Add(ad);
        }

        return ads;
    }

    private static async Task<Ad> GetAdAsync(IElementHandle card, ScrapJob scrapJob, WebsiteMetadata websiteMetadata)
    {
        var selectors = websiteMetadata.Selectors;

        var ad = new Ad();

        var titleElement = await card.QuerySelectorAsync(selectors.CardTitleSelector);
        ad.Title = titleElement is not null 
            ? await titleElement.InnerTextAsync() 
            : null;

        var priceElement = await card.QuerySelectorAsync(selectors.CardPriceSelector);
        ad.Price = priceElement is not null 
            ? await priceElement.InnerTextAsync() 
            : null;

        var locationDateElement = await card.QuerySelectorAsync(selectors.LocationAndDateSelector);
        ad.LocationAndDate = locationDateElement is not null 
            ? await locationDateElement.InnerTextAsync() 
            : null;

        var adUrlElement = await card.QuerySelectorAsync(selectors.AdUrlWrapperSelector);
        var partialUrl = adUrlElement is not null ? await adUrlElement.GetAttributeAsync(selectors.AdUrlSelector) : null;
        ad.Url = partialUrl is not null 
            ? websiteMetadata.Url + partialUrl 
            : null;

        var thumbnailElement = await card.QuerySelectorAsync(selectors.ThumbnailUrlWrapperSelector);
        ad.ThumbnailUrl = thumbnailElement is not null 
            ? await thumbnailElement.GetAttributeAsync(selectors.ThumbnailUrlSelector) 
            : null;

        // Url fallback
        if (ad.ThumbnailUrl is null)
        {
            thumbnailElement = await card.QuerySelectorAsync(selectors.BackupThumbnailUrlWrapperSelector);

            ad.ThumbnailUrl = thumbnailElement is not null 
                ? await thumbnailElement.GetAttributeAsync(selectors.BackupThumbnailUrlSelector) 
                : null;
        }

        return ad;
    }
}
