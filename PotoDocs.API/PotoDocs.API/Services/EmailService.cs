using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string plainTextContent, string htmlContent, CancellationToken cancellationToken = default);
}

public class EmailService : IEmailService
{
    private readonly EmailClient _emailClient;
    private readonly string _senderAddress;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _emailClient = new EmailClient(configuration["COMMUNICATION_SERVICES_CONNECTION_STRING"]);
        _senderAddress = configuration["SenderAddress"] ?? throw new ArgumentNullException(nameof(_senderAddress));
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string plainTextContent, string htmlContent, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(toEmail);
        ArgumentException.ThrowIfNullOrEmpty(subject);
        ArgumentException.ThrowIfNullOrEmpty(plainTextContent);
        ArgumentException.ThrowIfNullOrEmpty(htmlContent);

        var message = new EmailMessage(
            _senderAddress,
            new EmailRecipients(new List<EmailAddress> { new EmailAddress(toEmail) }),
            new EmailContent(subject)
            {
                PlainText = plainTextContent,
                Html = htmlContent
            });

        try
        {
            var operation = await _emailClient.SendAsync(WaitUntil.Completed, message, cancellationToken);
            _logger.LogInformation("Email sent to {Recipient}. Operation Id: {OperationId}", toEmail, operation.Id);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Błąd podczas wysyłania maila do {Recipient}", toEmail);
            throw new ApplicationException("Nie udało się wysłać wiadomości email. Spróbuj ponownie później.");
        }
    }
}
