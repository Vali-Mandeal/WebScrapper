using MailKit;
using MailKit.Net.Smtp;

using MimeKit;

namespace WebScrapper.Adapters;

public class SmtpClientAdapter : IDisposable
{
    private readonly SmtpClient _smtpClient;
    private readonly SmtpSettings _smtpSettings;

    public SmtpClientAdapter(SmtpClient smtpClient, SmtpSettings smtpSettings)
    {
        this._smtpClient = smtpClient;
        this._smtpSettings = smtpSettings;
    }


    public bool IsConnected => _smtpClient.IsConnected;
    public bool IsAuthenticated => _smtpClient.IsAuthenticated;

    public async Task ConnectAsync()
    {
        await _smtpClient.ConnectAsync(_smtpSettings.SmtpHost, _smtpSettings.SmtpPort, _smtpSettings.SecureSocketOptions);
    }
    public async Task AuthenticateAsync()
    {
        await _smtpClient.AuthenticateAsync(_smtpSettings.SenderEmail, _smtpSettings.SenderPassword);
    }
    public async Task<string> SendAsync(MimeMessage message, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return await _smtpClient.SendAsync(message, cancellationToken, progress);
    }



    private bool disposedValue = false;
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _smtpClient.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }
}
