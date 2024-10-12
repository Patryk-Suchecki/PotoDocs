using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using PotoDocs.API.Exceptions;
using PotoDocs.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PotoDocs.API.Services;

public interface IAccountService
{
    LoginResponseDto GenerateJwt(LoginDto dto);
    void RegisterUser(RegisterUserDto dto);
}

public class AccountService : IAccountService
{
    private readonly AuthenticationSettings _authSettings;
    private readonly PotodocsDbContext _context;
    private readonly IPasswordHasher<User> _hasher;

    public AccountService(PotodocsDbContext context, IPasswordHasher<User> hasher, AuthenticationSettings authenticationSettings)
    {
        _authSettings = authenticationSettings;
        _context = context;
        _hasher = hasher;
    }
    public void RegisterUser(RegisterUserDto dto)
    {
        var newUser = new User()
        {
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PasswordHash = dto.Password,
            Role = dto.Role
        };
        var hashedPassword = _hasher.HashPassword(newUser, dto.Password);
        newUser.PasswordHash = hashedPassword;
        _context.Users.Add(newUser);
        _context.SaveChanges();
    }

    public LoginResponseDto GenerateJwt(LoginDto dto)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);

        if (user is null)
        {
            throw new BadRequestException("Invalid username or password");
        }

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new BadRequestException("Invalid username or password");
        }

        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, $"{user.Role}"),

        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authSettings.JwtKey));
        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddDays(_authSettings.JwtExpireDays);

        var token = new JwtSecurityToken(_authSettings.JwtIssuer,
            _authSettings.JwtIssuer, claims, expires: expires, signingCredentials: cred);

        var tokenHandler = new JwtSecurityTokenHandler();

        return new LoginResponseDto()
        {
            Token = tokenHandler.WriteToken(token),
            Role = user.Role
        };
        
    }
}
