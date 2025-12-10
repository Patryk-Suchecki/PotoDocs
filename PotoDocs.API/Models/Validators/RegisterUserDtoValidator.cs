using FluentValidation;
using PotoDocs.API.Entities;
using PotoDocs.Shared.Models;
namespace PotoDocs.API.Models.Validators;

public class RegisterUserDtoValidator : AbstractValidator<UserDto>
{
    public RegisterUserDtoValidator(PotodocsDbContext dbContext)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .Custom((value, context) => 
            {
                var emailInUse = dbContext.Users.Any(u => u.Email == value);
                if (emailInUse)
                {
                    context.AddFailure("Email", "That email is taken");
                }
            });


    }
}
