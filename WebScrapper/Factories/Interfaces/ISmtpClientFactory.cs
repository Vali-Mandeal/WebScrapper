using MailKit.Net.Smtp;

namespace WebScrapper.Factories.Interfaces;

public interface ISmtpClientFactory
{
    public Task<SmtpClient> CreateAsync();
}
