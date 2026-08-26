using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AnimalRegistry.Modules.Contact.Infrastructure.Email;

internal sealed class SmtpEmailSender(IOptions<EmailSettings> settings, ILogger<SmtpEmailSender> logger)
    : IEmailSender
{
    private readonly EmailSettings _settings = settings.Value;

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var mimeMessage = BuildMimeMessage(message);

        using var client = new SmtpClient();
        client.Timeout = (int)TimeSpan.FromSeconds(_settings.TimeoutSeconds).TotalMilliseconds;

        var socketOptions = _settings.UseImplicitTls
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;

        await client.ConnectAsync(_settings.Host, _settings.Port, socketOptions, cancellationToken);

        if (!string.IsNullOrEmpty(_settings.UserName))
        {
            await client.AuthenticateAsync(_settings.UserName, _settings.Password ?? string.Empty, cancellationToken);
        }

        try
        {
            await client.SendAsync(mimeMessage, cancellationToken);
        }
        finally
        {
            await client.DisconnectAsync(true, cancellationToken);
        }

        logger.LogInformation("Contact notification sent over SMTP to {Host}:{Port}.", _settings.Host, _settings.Port);
    }

    private MimeMessage BuildMimeMessage(EmailMessage message)
    {
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(_settings.FromDisplayName, _settings.FromAddress));
        mimeMessage.To.Add(MailboxAddress.Parse(message.To));
        mimeMessage.Subject = message.Subject;

        if (message.ReplyTo is not null)
        {
            mimeMessage.ReplyTo.Add(new MailboxAddress(message.ReplyToDisplayName, message.ReplyTo));
        }

        mimeMessage.Body = new BodyBuilder
        {
            HtmlBody = message.HtmlBody,
            TextBody = message.TextBody,
        }.ToMessageBody();

        return mimeMessage;
    }
}