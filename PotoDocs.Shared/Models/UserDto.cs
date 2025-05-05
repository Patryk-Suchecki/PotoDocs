using System.ComponentModel.DataAnnotations;

namespace PotoDocs.Shared.Models;

public class UserDto
{
    public Guid Id { get; set; }
    [Required(ErrorMessage = "Imię jest wymagane.")]
    [RegularExpression(@"^[a-zA-ZĄĆĘŁŃÓŚŹŻąćęłńóśźż]+$", ErrorMessage = "Imię może zawierać tylko litery.")]
    [StringLength(50, ErrorMessage = "Imię może mieć maksymalnie 50 znaków.")]
    public string FirstName { get; set; }


    [Required(ErrorMessage = "Nazwisko jest wymagane.")]
    [RegularExpression(@"^[a-zA-ZĄĆĘŁŃÓŚŹŻąćęłńóśźż]+$", ErrorMessage = "Nazwisko może zawierać tylko litery.")]
    [StringLength(50, ErrorMessage = "Nazwisko może mieć maksymalnie 50 znaków.")]
    public string LastName { get; set; }


    [Required(ErrorMessage = "Email jest wymagany.")]
    [EmailAddress(ErrorMessage = "Podano nieprawidłowy adres email.")]
    public string Email { get; set; }


    [Required(ErrorMessage = "Rola jest wymagana.")]
    public string Role { get; set; }


    public string FirstAndLastName => $"{FirstName} {LastName}";
}
