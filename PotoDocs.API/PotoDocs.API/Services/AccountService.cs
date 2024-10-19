using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using PotoDocs.API.Exceptions;
using PotoDocs.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PotoDocs.Shared.Models;
using PotoDocs.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace PotoDocs.API.Services;

public interface IAccountService
{
    LoginResponseDto GenerateJwt(LoginDto dto);
    void RegisterUser(RegisterUserDto dto);
    void ChangePassword(ChangePasswordDto dto);
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
            Role = dto.Role
        };
        string randomPassword = GenerateRandomPassword(12);

        var hashedPassword = _hasher.HashPassword(newUser, randomPassword);
        newUser.PasswordHash = hashedPassword;
        _context.Users.Add(newUser);
        _context.SaveChanges();
    }

    public LoginResponseDto GenerateJwt(LoginDto dto)
    {
        var user = _context.Users.Include(u => u.Role).FirstOrDefault(u => u.Email == dto.Email);

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
            new Claim(ClaimTypes.Role, $"{user.Role.Name}"),

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

    public string GenerateRandomPassword(int length)
    {
        const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        var chars = new char[length];

        for (int i = 0; i < length; i++)
        {
            chars[i] = validChars[random.Next(0, validChars.Length)];
        }

        return new string(chars);
    }

    public void ChangePassword(ChangePasswordDto dto)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
        if (user is null)
        {
            throw new BadRequestException("User not found");
        }

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.OldPassword);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new BadRequestException("Old password is incorrect");
        }

        var newPasswordHash = _hasher.HashPassword(user, dto.NewPassword);
        user.PasswordHash = newPasswordHash;
        _context.SaveChanges();
    }
}
