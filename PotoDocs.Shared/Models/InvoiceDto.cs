using FluentValidation;
using PotoDocs.Shared.Models;

namespace PotoDocs.Shared.Models
{
    public class InvoiceDto
    {
        public Guid Id { get; set; }
        public int InvoiceNumber { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? SaleDate { get; set; }
        public DateTime? SentDate { get; set; }
        public DeliveryMethodType? DeliveryMethod { get; set; }
        public bool HasPaid { get; set; } = false;


        public string BuyerName { get; set; } = string.Empty;
        public string BuyerAddress { get; set; } = string.Empty;
        public string BuyerNIP { get; set; } = string.Empty;


        public Guid? OrderId { get; set; }


        public decimal TotalNetAmount { get; set; }
        public decimal TotalVatAmount { get; set; }
        public decimal TotalGrossAmount { get; set; }
        public CurrencyType Currency { get; set; } = CurrencyType.EUR;


        public string PaymentMethod { get; set; } = string.Empty;
        public int? PaymentDeadlineDays { get; set; }

        public ICollection<InvoiceItemDto> Items { get; set; } = [];

        public string Name => $"{InvoiceNumber}/{IssueDate:MM}/{IssueDate:yyyy}";
        public int DaysUntilDue => PaymentDeadlineDays.HasValue && IssueDate.HasValue ? (IssueDate.Value.AddDays(PaymentDeadlineDays.Value) - DateTime.UtcNow).Days : 0;
    }
}
public class InvoiceDtoValidator : AbstractValidator<InvoiceDto>
{
    public InvoiceDtoValidator()
    {
        RuleFor(x => x.InvoiceNumber)
            .GreaterThan(0).WithMessage("Numer faktury jest wymagany i musi być większy od 0.");

        RuleFor(x => x.IssueDate)
            .NotEmpty().WithMessage("Data wystawienia jest wymagana.");

        RuleFor(x => x.SaleDate)
            .NotEmpty().WithMessage("Data sprzedaży jest wymagana.");

        RuleFor(x => x.BuyerName)
            .NotEmpty().WithMessage("Nazwa nabywcy jest wymagana.")
            .MaximumLength(255).WithMessage("Nazwa nabywcy nie może przekraczać 255 znaków.");

        RuleFor(x => x.BuyerAddress)
            .NotEmpty().WithMessage("Adres nabywcy jest wymagany.")
            .MaximumLength(500).WithMessage("Adres nabywcy nie może przekraczać 500 znaków.");

        RuleFor(x => x.BuyerNIP)
            .NotEmpty().WithMessage("NIP nabywcy jest wymagany.")
            .MaximumLength(50).WithMessage("NIP nabywcy nie może przekraczać 50 znaków.");

        RuleFor(x => x.TotalNetAmount)
            .GreaterThan(0).WithMessage("Wartość netto musi być większa od 0.");

        RuleFor(x => x.TotalGrossAmount)
            .GreaterThan(0).WithMessage("Wartość brutto musi być większa od 0.");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Metoda płatności jest wymagana.");

        RuleFor(x => x.PaymentDeadlineDays)
            .GreaterThanOrEqualTo(0).WithMessage("Termin płatności nie może być ujemny (0 = płatne od ręki).");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Faktura musi zawierać co najmniej jedną pozycję.");

        RuleForEach(x => x.Items)
            .SetValidator(new InvoiceItemDtoValidator());
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<InvoiceDto>.CreateWithOptions((InvoiceDto)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return [];
        return result.Errors.Select(e => e.ErrorMessage);
    };
}
public enum CurrencyType
{
    EUR,
    PLN
}
public enum DeliveryMethodType
{
    Post,
    Email,
}