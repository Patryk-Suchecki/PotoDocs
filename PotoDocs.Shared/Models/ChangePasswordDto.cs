using FluentValidation;

namespace PotoDocs.Shared.Models;
public class ChangePasswordDto
{
    public required string Email { get; set; }
    public required string OldPassword { get; set; }
    public required string NewPassword { get; set; }
}
public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email jest wymagany.");

        RuleFor(x => x.OldPassword)
            .NotEmpty().WithMessage("Stare hasło jest wymagane.")
            .Length(8, 50).WithMessage("Stare hasło musi mieć od 8 do 50 znaków.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Nowe hasło jest wymagane.")
            .Length(8, 50).WithMessage("Nowe hasło musi mieć od 8 do 50 znaków.")
            .NotEqual(x => x.OldPassword).WithMessage("Nowe hasło nie może być takie samo jak stare hasło.");
    }
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<ChangePasswordDto>.CreateWithOptions((ChangePasswordDto)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}