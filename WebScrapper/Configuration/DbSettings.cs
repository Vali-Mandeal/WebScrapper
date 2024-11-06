namespace WebScrapper.Configuration;
public class DbSettings
{
    public const string Key = "DbSettings";
    public string MongoUrl { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}
