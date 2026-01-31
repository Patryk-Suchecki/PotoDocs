using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Options;
using PotoDocs.API.Options;

namespace PotoDocs.API.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlContent, string? plainTextContent = null, IEnumerable<EmailAttachment>? attachments = null, CancellationToken cancellationToken = default);
}

public class EmailService(EmailClient emailClient, IOptions<EmailServiceOptions> emailOptions) : IEmailService
{
    private readonly EmailClient _emailClient = emailClient;

    private readonly string _senderAddress = emailOptions.Value.SenderAddress
        ?? throw new InvalidOperationException("EmailService: SenderAddress is not configured.");

    private readonly string _senderDisplayName = emailOptions.Value.SenderDisplayName
        ?? "PotoDocs";

    public async Task SendEmailAsync(string toEmail, string subject, string htmlContent, string? plainTextContent = null, IEnumerable<EmailAttachment>? attachments = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(toEmail);
        ArgumentException.ThrowIfNullOrEmpty(subject);
        ArgumentException.ThrowIfNullOrEmpty(htmlContent);

        var emailContent = new EmailContent(subject)
        {
            Html = htmlContent,
            PlainText = plainTextContent
        };

        var message = new EmailMessage(
            _senderAddress,
            new EmailRecipients([new EmailAddress(toEmail)]),
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
            await _emailClient.SendAsync(WaitUntil.Started, message, cancellationToken);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Nie udało się wysłać wiadomości email do {toEmail}. Azure Error: {ex.Message}", ex);
        }
    }
}