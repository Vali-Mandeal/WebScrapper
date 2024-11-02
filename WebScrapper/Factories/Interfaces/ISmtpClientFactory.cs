using WebScrapper.Adapters;

namespace WebScrapper.Factories.Interfaces;

public interface ISmtpClientFactory
{
    public Task<SmtpClientAdapter> CreateAsync();
}
