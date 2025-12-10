using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Options;
using PotoDocs.API.Options;

namespace PotoDocs.API.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlContent, string? plainTextContent = null, IEnumerable<EmailAttachment>? attachments = null, CancellationToken cancellationToken = default);
}

public class EmailService : IEmailService
{
    private readonly EmailClient _emailClient;

    private readonly string _senderAddress;
    private readonly string _senderDisplayName;

    public EmailService(EmailClient emailClient, IOptions<EmailServiceOptions> emailOptions)
    {
        _emailClient = emailClient;

        _senderDisplayName = emailOptions.Value.SenderDisplayName ??
            throw new InvalidOperationException("EmailService: SenderDisplayName is not configured.");

        _senderAddress = emailOptions.Value.SenderAddress ??
            throw new InvalidOperationException("EmailService: SenderAddress is not configured.");

        if (string.IsNullOrEmpty(emailOptions.Value.ConnectionString))
        {
            throw new InvalidOperationException("EmailService: ConnectionString is not configured.");
        }
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlContent, string? plainTextContent = null, IEnumerable<EmailAttachment>? attachments = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(toEmail);
        ArgumentException.ThrowIfNullOrEmpty(subject);
        ArgumentException.ThrowIfNullOrEmpty(htmlContent);

        var emailContent = new EmailContent(subject)
        {
            Html = htmlContent
        };

        if (!string.IsNullOrEmpty(plainTextContent))
        {
            emailContent.PlainText = plainTextContent;
        }

        var message = new EmailMessage(
            _senderAddress,
            new EmailRecipients([new(toEmail)]),
            emailContent
        );

        message.ReplyTo.Add(new EmailAddress(_senderAddress, _senderDisplayName));


        if (attachments != null)
        {
            foreach (var attachment in attachments)
            {
                message.Attachments.Add(attachment);
            }
        }

        try
        {
            await _emailClient.SendAsync(WaitUntil.Completed, message, cancellationToken);
        }
        catch (RequestFailedException ex)
        {
            throw new ApplicationException($"Nie udało się wysłać wiadomości email do {toEmail}. Błąd: {ex.Message}", ex);
        }
    }
}