namespace WebScrapper.Shared.Entities;
public record Notification(List<NotificationReceiver> Receivers, string Subject, string Body, string Job);
