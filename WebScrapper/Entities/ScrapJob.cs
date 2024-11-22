namespace WebScrapper.Entities;
public class ScrapJob
{
    public int Id { get; set; }
    public int WebsiteMetadataId { get; set; }
    public string Name { get; set; } = "";
    public string SearchValue { get; set; } = "";
    public List<string> MustContainList { get; set; } = [];
    public List<string> MustNotContainList { get; set; } = [];
    public decimal MaxPrice { get; set; }
    public List<NotificationReceiver> NotificationReceivers { get; set; } = new List<NotificationReceiver>();
    public DateTime CreatedOn { get; set; }
}
