using WebScrapper.Shared.Entities;

namespace WebScrapper.Slave.Repositories.Interfaces;
public interface INotificationRepository
{
    Task SendNotificationAsync(Notification notification);
}
