using MailKit.Security;

namespace WebScrapper.Configuration;

public class SmtpSettings
{
    public const string Key = "SmtpSettings";
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderPassword { get; set; } = string.Empty;
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public SecureSocketOptions SecureSocketOptions { get; set; }
}
