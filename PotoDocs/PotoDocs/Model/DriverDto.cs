using System.Text.Json.Serialization;

namespace PotoDocs.Model;

public class DriverDto
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
}
[JsonSerializable(typeof(List<DriverDto>))]
internal sealed partial class DriverDtoContext : JsonSerializerContext { }