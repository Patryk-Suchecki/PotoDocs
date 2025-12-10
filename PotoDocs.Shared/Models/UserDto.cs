using FluentValidation;

namespace PotoDocs.Shared.Models;

public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string FirstAndLastName => $"{FirstName} {LastName}";
    public override string ToString()
    {
        return FirstAndLastName;
    }
}
public class UserDtoValidator : AbstractValidator<UserDto>
{
    public UserDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Imię jest wymagane.")
            .MaximumLength(50).WithMessage("Imię może mieć maksymalnie 50 znaków.")
            .Matches(@"^[\p{L}\s\-']+$").WithMessage("Imię może zawierać tylko litery.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Nazwisko jest wymagane.")
            .MaximumLength(50).WithMessage("Nazwisko może mieć maksymalnie 50 znaków.")
            .Matches(@"^[\p{L}\s\-']+$").WithMessage("Nazwisko może zawierać tylko litery.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email jest wymagany.")
            .EmailAddress().WithMessage("Podano nieprawidłowy adres email.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Rola jest wymagana.");
    }
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<UserDto>.CreateWithOptions((UserDto)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return [];
        return result.Errors.Select(e => e.ErrorMessage);
    };
}