using MailKit.Security;

public class SmtpSettings
{
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderPassword { get; set; } = string.Empty;
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public SecureSocketOptions SecureSocketOptions { get; set; }
}
