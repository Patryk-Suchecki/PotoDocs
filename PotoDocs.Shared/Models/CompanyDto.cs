using FluentValidation;

namespace PotoDocs.Shared.Models;

public class CompanyDto
{
    public string NIP { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string CorrespondenceAddress { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool AcceptsEInvoices { get; set; }
    public string EmailAddress { get; set; } = string.Empty;
}
public class CompanyDtoValidator : AbstractValidator<CompanyDto>
{
    public CompanyDtoValidator()
    {
        RuleFor(x => x.NIP)
            .NotEmpty().WithMessage("NIP jest wymagany.")
            .MaximumLength(15).WithMessage("NIP firmy nie może mieć więcej niż 15 znaków.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nazwa firmy jest wymagana.")
            .MaximumLength(100).WithMessage("Nazwa firmy nie może mieć więcej niż 100 znaków.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Adres jest wymagany.")
            .MaximumLength(200).WithMessage("Adres firmy nie może mieć więcej niż 200 znaków.");

        RuleFor(x => x.CorrespondenceAddress)
            .MaximumLength(200).WithMessage("Adres korespondencyjny firmy nie może mieć więcej niż 200 znaków.");

        RuleFor(x => x.Country)
            .MaximumLength(50).WithMessage("Kraj firmy nie może mieć więcej niż 50 znaków.");
    }
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<CompanyDto>.CreateWithOptions((CompanyDto)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}