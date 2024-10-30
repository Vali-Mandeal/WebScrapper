using WebScrapper.Entities;

namespace WebScrapper.Repositories.Interfaces;
public interface INotificationRepository
{
    Task SendNotificationAsync(Notification notification);
}
