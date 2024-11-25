using System.ComponentModel.DataAnnotations;

namespace PotoDocs.Shared.Models;

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Email jest wymagany.")]
    [EmailAddress(ErrorMessage = "Podaj poprawny adres e-mail.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Stare hasło jest wymagane.")]
    [StringLength(50, MinimumLength = 8, ErrorMessage = "Stare hasło musi mieć od 8 do 50 znaków.")]
    public string OldPassword { get; set; }

    [Required(ErrorMessage = "Nowe hasło jest wymagane.")]
    [StringLength(50, MinimumLength = 8, ErrorMessage = "Nowe hasło musi mieć od 8 do 50 znaków.")]
    public string NewPassword { get; set; }
}
