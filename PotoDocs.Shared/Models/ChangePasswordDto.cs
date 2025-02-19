using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

public class ChangePasswordDto : IValidatableObject
{
    public string Email { get; set; }

    [Required(ErrorMessage = "Stare hasło jest wymagane.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Stare hasło musi mieć od 8 do 50 znaków.")]
    public string OldPassword { get; set; }

    [Required(ErrorMessage = "Nowe hasło jest wymagane.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Nowe hasło musi mieć od 8 do 50 znaków.")]
    public string NewPassword { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (OldPassword == NewPassword)
        {
            yield return new ValidationResult("Nowe hasło nie może być takie samo jak stare hasło.", new[] { nameof(NewPassword) });
        }
    }
}