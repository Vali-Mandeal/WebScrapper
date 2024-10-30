namespace WebScrapper.Entities;
public class Ad
{
    public int? Id { get; set; }
    public int? ScrapJobId { get; set; }
    public string? Title { get; set; }
    public string? Price { get; set; }
    public string? LocationAndDate { get; set; }
    public string? Url { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool ShouldSendNotification { get; set; }
    public bool NotificationSent { get; set; }
}
