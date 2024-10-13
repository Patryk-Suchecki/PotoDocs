using System.Text.Json.Serialization;
using PotoDocs.Shared.Models;

namespace PotoDocs.Shared.Models;

public class RegisterUserDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public Role Role { get; set; } = Role.Driver;
}
[JsonSerializable(typeof(List<RegisterUserDto>))]
internal sealed partial class RegisterUserDtoContext : JsonSerializerContext { }
