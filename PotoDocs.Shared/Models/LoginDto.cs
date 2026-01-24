using FluentValidation;

namespace PotoDocs.Shared.Models;

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email jest wymagany.")
            .EmailAddress().WithMessage("Nieprawidłowy adres email.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Hasło jest wymagane.")
            .MaximumLength(50).WithMessage("Hasło musi mieć do 50 znaków.");
    }
}