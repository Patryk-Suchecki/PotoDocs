using Microsoft.AspNetCore.Identity;
using PotoDocs.API.Models;
using PotoDocs.Shared.Models;
using PotoDocs.API.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System.Net;

namespace PotoDocs.API.Services;

public interface IUserService
{
    ApiResponse<string> Register(UserDto dto);
    ApiResponse<string> ChangePassword(ChangePasswordDto dto);
    ApiResponse<string> GeneratePassword(string email);
    ApiResponse<List<UserDto>> GetAll();
    ApiResponse<UserDto> GetById(Guid id);
    void Delete(string email);
}

public class UserService : IUserService
{
    private readonly PotodocsDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IMapper _mapper;

    public UserService(PotodocsDbContext context, IPasswordHasher<User> hasher, IMapper mapper, ITokenService tokenService, IEmailService emailService)
    {
        _context = context;
        _hasher = hasher;
        _mapper = mapper;
        _emailService = emailService;
    }
    public ApiResponse<string> Register(UserDto dto)
    {
        var role = _context.Roles.FirstOrDefault(r => r.Name == dto.Role);

        if (role == null)
        {
            return ApiResponse<string>.Failure($"Rola '{dto.Role}' nie istnieje.", HttpStatusCode.BadRequest);
        }

        var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
        if (user == null)
        {
            user = _mapper.Map<User>(dto);
            string randomPassword = GenerateRandomPassword(12);

            var placeholders = new Dictionary<string, string>
        {
            { "email", dto.Email },
            { "password", randomPassword },
            {"name", dto.FirstName },
            { "lastname", dto.LastName }
        };

            string emailBody = LoadEmailTemplate("welcome.html", placeholders);

            _emailService.SendEmail(dto.Email, "Witaj w PotoDocs 🚚", "Twoje dane do logowania", emailBody);

            var hashedPassword = _hasher.HashPassword(user, randomPassword);
            user.PasswordHash = hashedPassword;
            _context.Users.Add(user);
        }
        else
        {
            _mapper.Map(dto, user);
        }

        user.Role = role;
        _context.SaveChanges();

        return ApiResponse<string>.Success(HttpStatusCode.Created);
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

    public ApiResponse<string> ChangePassword(ChangePasswordDto dto)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
        if (user is null)
        {
            return ApiResponse<string>.Failure($"Nie znaleziono użytkownika", HttpStatusCode.BadRequest);
        }
        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.OldPassword);
        if (result == PasswordVerificationResult.Failed)
        {
            return ApiResponse<string>.Failure("Nieprawidłowe hasło", HttpStatusCode.Unauthorized);
        }
        var newPasswordHash = _hasher.HashPassword(user, dto.NewPassword);

        var placeholders = new Dictionary<string, string>
    {
        { "email", user.Email },
        { "password", dto.NewPassword },
        { "name", user.FirstName },
        { "lastname", user.LastName }
    };

        string emailBody = LoadEmailTemplate("reset-password.html", placeholders);

        _emailService.SendEmail(user.Email, "Resetowanie hasła", "Twoje nowe hasło", emailBody);

        user.PasswordHash = newPasswordHash;
        _context.SaveChanges();
        return ApiResponse<string>.Success(HttpStatusCode.OK);
    }
    public ApiResponse<string> GeneratePassword(string email)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user is null)
        {
            return ApiResponse<string>.Failure($"Nie znaleziono użytkownika", HttpStatusCode.BadRequest);
        }

        string randomPassword = GenerateRandomPassword(12);

        var newPasswordHash = _hasher.HashPassword(user, randomPassword);
        user.PasswordHash = newPasswordHash;
        _context.SaveChanges();

        return ApiResponse<string>.Success(HttpStatusCode.OK);
    }

    public ApiResponse<List<UserDto>> GetAll()
    {
        var users = _context.Users.Include(u => u.Role).ToList();

        var usersDto = _mapper.Map<List<UserDto>>(users);
        return ApiResponse<List<UserDto>>.Success(usersDto);
    }

    public ApiResponse<UserDto> GetById(Guid id)
    {
        var user = _context.Users.Include(u => u.Role).FirstOrDefault(u => u.Id == id);
        if (user == null) return ApiResponse<UserDto>.Failure("Nie znaleziono użytkownika", HttpStatusCode.Unauthorized);

        var dto = _mapper.Map<UserDto>(user);

        return ApiResponse<UserDto>.Success(dto);
    }
    public void Delete(string email)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user == null) return;

        _context.Users.Remove(user);
        _context.SaveChanges();
    }
    private string LoadEmailTemplate(string templateName, Dictionary<string, string> placeholders)
    {
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "emails", templateName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Nie znaleziono szablonu e-maila: {filePath}");
        }

        string templateContent = File.ReadAllText(filePath);

        foreach (var placeholder in placeholders)
        {
            templateContent = templateContent.Replace($"{{{placeholder.Key}}}", placeholder.Value);
        }

        return templateContent;
    }

}

