namespace PotoDocs.API.Options;

public class AuthenticationSettings
{
    public string JwtKey { get; set; } = string.Empty;
    public string JwtIssuer { get; set; } = string.Empty;
    public int JwtExpireInMinutes { get; set; }
}
