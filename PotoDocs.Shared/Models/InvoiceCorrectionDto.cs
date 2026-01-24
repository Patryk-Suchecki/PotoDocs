using FluentValidation;
using PotoDocs.Shared.Models;

namespace PotoDocs.Shared.Models
{
    public class InvoiceCorrectionDto
    {
        public Guid Id { get; set; }
        public int InvoiceNumber { get; set; }
        public DateTime? IssueDate { get; set; } = DateTime.UtcNow;
        public DateTime? SentDate { get; set; }
        public DeliveryMethodType? DeliveryMethod { get; set; }
        public bool HasPaid { get; set; } = false;
        public string Comments { get; set; } = "Błędnie wystawiona faktura";
        public required InvoiceDto OriginalInvoice { get; set; }

        public ICollection<InvoiceItemDto> Items { get; set; } = [];

        public string Name => $"{InvoiceNumber}/{IssueDate:MM}/{IssueDate:yyyy}/K";
    }
}
public class InvoiceCorrectionDtoValidator : AbstractValidator<InvoiceCorrectionDto>
{
    public InvoiceCorrectionDtoValidator()
    {
        RuleFor(x => x.InvoiceNumber)
            .GreaterThan(0).WithMessage("Numer korekty jest wymagany i musi być większy od 0.");

        RuleFor(x => x.IssueDate)
            .NotEmpty().WithMessage("Data wystawienia jest wymagana.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Faktura musi zawierać co najmniej jedną pozycję.");

        RuleForEach(x => x.Items)
            .SetValidator(new InvoiceItemDtoValidator());
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<InvoiceCorrectionDto>.CreateWithOptions((InvoiceCorrectionDto)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return [];
        return result.Errors.Select(e => e.ErrorMessage);
    };
}