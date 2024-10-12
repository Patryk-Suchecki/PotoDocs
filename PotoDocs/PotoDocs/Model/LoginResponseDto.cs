namespace PotoDocs.Model;

public class LoginResponseDto
{
    public UserRole Role { get; set; }
    public string Token { get; set; }
}
public enum UserRole
{
    Admin,
    User,
}
