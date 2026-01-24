using FluentValidation;

namespace PotoDocs.Shared.Models;

public class InvoiceItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public string Unit { get; set; } = "Usługa";
    public decimal NetPrice { get; set; }
    public decimal NetValue { get; set; }
    public decimal VatRate { get; set; } = 0.23m;
    public decimal VatAmount { get; set; }
    public decimal GrossValue { get; set; }
}
public class InvoiceItemDtoValidator : AbstractValidator<InvoiceItemDto>
{
    public InvoiceItemDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nazwa pozycji jest wymagana.")
            .MaximumLength(200).WithMessage("Nazwa pozycji nie może przekraczać 200 znaków.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Ilość musi być większa od 0.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Jednostka miary (np. 'Usługa') jest wymagana.");

        RuleFor(x => x.VatRate)
            .GreaterThanOrEqualTo(0).WithMessage("Stawka VAT nie może być ujemna.");
    }
}