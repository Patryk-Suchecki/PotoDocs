using Azure.Communication.Email;
using PotoDocs.API.Entities;
using PotoDocs.API.Services;
using PotoDocs.Shared.Models;
using System.Text;

namespace PotoDocs.API.Services;

public interface IOrderDocumentSender
{
    Task SendDocumentsViaEmailAsync(Invoice invoiceWithDetails);
}

public class OrderDocumentSender(IEmailService emailService, IInvoiceService invoiceService, IFileStorageService fileStorage) : IOrderDocumentSender
{
    private readonly IEmailService _emailService = emailService;
    private readonly IInvoiceService _invoiceService = invoiceService;
    private readonly IFileStorageService _fileStorage = fileStorage;

    public async Task SendDocumentsViaEmailAsync(Invoice invoice)
    {
        var order = invoice.Order
            ?? throw new ArgumentException("Faktura musi zawierać załadowane Zlecenie.");

        if (string.IsNullOrWhiteSpace(order.Company.EmailAddress))
            throw new InvalidOperationException($"Firma {order.Company.Name} nie ma zdefiniowanego adresu e-mail.");

        var documentToSend = invoice.Corrections
            .OrderByDescending(c => c.IssueDate)
            .FirstOrDefault() ?? invoice;

        var attachments = new List<EmailAttachment>();

        var invoiceResult = await _invoiceService.GetInvoiceStreamAsync(documentToSend.Id);

        using (var pdfStream = invoiceResult.FileStream)
        {
            var pdfContent = await BinaryData.FromStreamAsync(pdfStream);
            attachments.Add(new EmailAttachment(invoiceResult.FileName, invoiceResult.ContentType, pdfContent));
        }

        var cmrFiles = order.Files.Where(f => f.Type == FileType.Cmr).ToList();
        foreach (var cmr in cmrFiles)
        {
            var fileNameOnDisk = $"{cmr.Id}{cmr.Extension}";
            var (cmrStream, mime) = await _fileStorage.GetFileStreamAsync(cmr.Path, fileNameOnDisk);

            using (cmrStream)
            {
                var cmrContent = await BinaryData.FromStreamAsync(cmrStream);
                attachments.Add(new EmailAttachment($"{cmr.Name}{cmr.Extension}", mime, cmrContent));
            }
        }

        string htmlBody = await LoadEmailTemplate(documentToSend, order, cmrFiles.Count > 0);
        string subject = $"Dokumenty do zlecenia: {order.OrderNumber ?? documentToSend.InvoiceNumber.ToString()}";

        await _emailService.SendEmailAsync(order.Company.EmailAddress, subject, htmlBody, "Dokumenty w załączniku.", attachments);
    }

    private async Task<string> LoadEmailTemplate(Invoice invoice, Order order, bool hasCmr)
    {
        var (fileStream, _) = await _fileStorage.GetFileStreamAsync(FileType.EmailTemplate, "invoice-documents.html");

        string template;
        using (fileStream)
        using (var reader = new StreamReader(fileStream, Encoding.UTF8))
        {
            template = await reader.ReadToEndAsync();
        }
        string cmrHtml = hasCmr ? "<li style=\"padding: 5px 0;\">✔️ CMR / Proof of Delivery</li>" : "";

        string invoiceNo = $"{invoice.Name}";

        return template
            .Replace("{clientName}", order.Company.Name)
            .Replace("{orderNumber}", order.OrderNumber)
            .Replace("{invoiceNumber}", invoiceNo)
            .Replace("{cmrLine}", cmrHtml);
    }
}