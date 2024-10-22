using System.Text.Json.Serialization;
using PotoDocs.Shared.Models;

namespace PotoDocs.Shared.Models;

public class UserDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public Role Role { get; set; }
    public string FirstAndLastName
    {
        get { return $"{FirstName} {LastName}"; }
    }
}

[JsonSerializable(typeof(List<UserDto>))]
internal sealed partial class RegisterUserDtoContext : JsonSerializerContext { }
