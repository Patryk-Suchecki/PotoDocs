namespace PotoDocs.Shared.Models;
 
public class LoginResponseDto
{
    public required string Role { get; set; }
    public required string Token { get; set; }
}
