using WebScrapper.Shared.Entities;

namespace WebScrapper.Slave.Services.Interfaces;
public interface INotificationService
{
    Task SendNotificationAsync(List<Ad> ads, ScrapJob scrapJob);
}
