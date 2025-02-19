using System.ComponentModel.DataAnnotations;

namespace PotoDocs.Shared.Models;

public class LoginDto
{
    [Required(ErrorMessage = "Email jest wymagany.")]
    [EmailAddress(ErrorMessage = "Nieprawidłowy adres email.")]
    public string Email { get; set; }


    [Required(ErrorMessage = "Hasło jest wymagane.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Hasło musi mieć od 8 do 50 znaków.")]
    public string Password { get; set; }
}
