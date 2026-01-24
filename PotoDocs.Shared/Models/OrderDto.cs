using FluentValidation;

namespace PotoDocs.Shared.Models;

public class OrderDto
{

    public Guid Id { get; set; }
    public DateTime? UnloadingDate { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public UserDto? Driver { get; set; }
    public CompanyDto Company { get; set; } = new CompanyDto();

    public decimal Price { get; set; }
    public CurrencyType Currency { get; set; } = CurrencyType.EUR;
    public int? PaymentDeadline { get; set; }

    public List<OrderStopDto> Stops { get; set; } = [];
    public List<OrderFileDto> Files { get; set; } = [];
    public InvoiceDto? Invoice { get; set; }

}
public class OrderDtoValidator : AbstractValidator<OrderDto>
{
    public OrderDtoValidator()
    {
        RuleFor(x => x.UnloadingDate)
            .NotNull().WithMessage("Data wystawienia jest wymagana.");

        RuleFor(x => x.Price)
            .GreaterThan(0m).WithMessage("Cena musi być większa od 0.");

        RuleFor(x => x.PaymentDeadline)
            .GreaterThan(0).WithMessage("Termin płatności jest wymagany.");

        RuleFor(x => x.OrderNumber)
            .MaximumLength(200).WithMessage("Numer zamówienia firmy nie może mieć więcej niż 200 znaków.");

        RuleFor(x => x.Company)
            .SetValidator(new CompanyDtoValidator());

        RuleForEach(x => x.Stops)
            .SetValidator(new OrderStopDtoValidator());
    }
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<OrderDto>.CreateWithOptions((OrderDto)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return [];
        return result.Errors.Select(e => e.ErrorMessage);
    };
}