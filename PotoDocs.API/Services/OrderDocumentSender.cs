using Azure.Communication.Email;
using PotoDocs.API.Entities;
using PotoDocs.API.Services;
using PotoDocs.Shared.Models;

namespace PotoDocs.API.Services;

public interface IOrderDocumentSender
{
    Task SendDocumentsViaEmailAsync(Invoice invoiceWithDetails);
}

public class OrderDocumentSender(IEmailService emailService, IInvoiceService invoiceService, IFileStorageService fileStorage, IWebHostEnvironment env) : IOrderDocumentSender
{
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

        var (pdfBytes, mimeType, pdfName) = await invoiceService.GetInvoiceFileAsync(documentToSend.Id);
        attachments.Add(new EmailAttachment(pdfName, mimeType, new BinaryData(pdfBytes)));

        var cmrFiles = order.Files.Where(f => f.Type == FileType.Cmr).ToList();
        foreach (var cmr in cmrFiles)
        {
            var fileNameOnDisk = $"{cmr.Id}{cmr.Extension}";
            var (bytes, mime) = await fileStorage.GetFileAsync(cmr.Path, fileNameOnDisk);

            attachments.Add(new EmailAttachment($"{cmr.Name}{cmr.Extension}", mime, new BinaryData(bytes)));
        }

        string htmlBody = await LoadEmailTemplate(documentToSend, order, cmrFiles.Count > 0);
        string subject = $"Dokumenty do zlecenia: {order.OrderNumber ?? documentToSend.InvoiceNumber.ToString()}";

        await emailService.SendEmailAsync(order.Company.EmailAddress, subject, htmlBody, "Dokumenty w załączniku.", attachments);
    }

    private async Task<string> LoadEmailTemplate(Invoice invoice, Order order, bool hasCmr)
    {
        var path = Path.Combine(env.WebRootPath, "emails", "invoice-documents.html");
        if (!File.Exists(path)) throw new FileNotFoundException("Brak szablonu email.");

        var template = await File.ReadAllTextAsync(path);

        string cmrHtml = hasCmr ? "<li style=\"padding: 5px 0;\">✔️ CMR / Proof of Delivery</li>" : "";
        string invoiceNo = $"{invoice.InvoiceNumber}/{invoice.IssueDate:MM'/'yyyy}";

        return template
            .Replace("{clientName}", order.Company.Name)
            .Replace("{orderNumber}", order.OrderNumber)
            .Replace("{invoiceNumber}", invoiceNo)
            .Replace("{cmrLine}", cmrHtml);
    }
}