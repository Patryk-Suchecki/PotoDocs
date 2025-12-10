using FluentValidation;

namespace PotoDocs.Shared.Models;

public class OrderStopDto
{
    public int Id { get; set; }
    public StopType Type { get; set; }
    public DateTime? Date { get; set; } = DateTime.Now;
    public string Address { get; set; } = string.Empty;
}
public class OrderStopDtoValidator : AbstractValidator<OrderStopDto>
{
    public OrderStopDtoValidator()
    {
        RuleFor(x => x.Date)
            .NotNull().WithMessage("Data jest wymagana.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Adres jest wymagany.")
            .MaximumLength(200).WithMessage("Adres rozładunku nie może mieć więcej niż 200 znaków.");
    }
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<OrderStopDto>.CreateWithOptions((OrderStopDto)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return [];
        return result.Errors.Select(e => e.ErrorMessage);
    };
}
public enum StopType
{
    Loading,
    Unloading
}