using System.Text.Json.Serialization;
using PotoDocs.Shared.Models;

namespace PotoDocs.Shared.Models;

public class RegisterUserDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public Role Role { get; set; }
}

[JsonSerializable(typeof(List<RegisterUserDto>))]
internal sealed partial class RegisterUserDtoContext : JsonSerializerContext { }
