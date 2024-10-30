using WebScrapper.Entities;

namespace WebScrapper.Services.Interfaces;
public interface INotificationService
{
    Task SendNotificationAsync(List<Ad> ads, ScrapJob scrapJob);
}
