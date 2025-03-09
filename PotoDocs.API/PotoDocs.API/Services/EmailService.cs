using Azure;
using Azure.Communication.Email;

public interface IEmailService
{
    void SendEmail(string toEmail, string subject, string plainTextContent, string htmlContent);
}

public class EmailService : IEmailService
{
    private readonly EmailClient _emailClient;
    private readonly string _senderAddress;

    public EmailService(IConfiguration configuration)
    {
        _emailClient = new EmailClient(configuration["COMMUNICATION_SERVICES_CONNECTION_STRING"]);
        _senderAddress = configuration["SenderAddress"];
    }

    public void SendEmail(string toEmail, string subject, string plainTextContent, string htmlContent)
    {
        if (string.IsNullOrEmpty(toEmail))
            throw new ArgumentException("Recipient email cannot be null or empty.", nameof(toEmail));
        if (string.IsNullOrEmpty(subject))
            throw new ArgumentException("Email subject cannot be null or empty.", nameof(subject));
        if (string.IsNullOrEmpty(plainTextContent))
            throw new ArgumentException("Plain text content cannot be null or empty.", nameof(plainTextContent));
        if (string.IsNullOrEmpty(htmlContent))
            throw new ArgumentException("HTML content cannot be null or empty.", nameof(htmlContent));

        var emailMessage = new EmailMessage(
            senderAddress: _senderAddress,
            content: new EmailContent(subject)
            {
                PlainText = plainTextContent,
                Html = htmlContent
            },
            recipients: new EmailRecipients(new List<EmailAddress> { new EmailAddress(toEmail) }));

        try
        {
            EmailSendOperation emailSendOperation = _emailClient.Send(WaitUntil.Completed, emailMessage);
        }
        catch (RequestFailedException ex)
        {
            throw;
        }
    }
}
