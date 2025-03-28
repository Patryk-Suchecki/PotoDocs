using Microsoft.AspNetCore.Identity;
using PotoDocs.API.Models;
using PotoDocs.Shared.Models;
using PotoDocs.API.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System.Net;
using PotoDocs.API.Exceptions;

namespace PotoDocs.API.Services;

public interface IUserService
{
    Task RegisterAsync(UserDto dto);
    Task ChangePasswordAsync(ChangePasswordDto dto);
    Task GeneratePasswordAsync(string email);
    Task<List<UserDto>> GetAllAsync();
    Task<UserDto> GetByIdAsync(Guid id);
    Task DeleteAsync(string email);
}



public class UserService : IUserService
{
    private readonly PotodocsDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IMapper _mapper;

    public UserService(
        PotodocsDbContext context,
        IPasswordHasher<User> hasher,
        IMapper mapper,
        ITokenService tokenService,
        IEmailService emailService)
    {
        _context = context;
        _hasher = hasher;
        _mapper = mapper;
        _emailService = emailService;
    }

    public async Task RegisterAsync(UserDto dto)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == dto.Role)
            ?? throw new BadRequestException($"Rola '{dto.Role}' nie istnieje.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null)
        {
            user = _mapper.Map<User>(dto);
            string randomPassword = GenerateRandomPassword(12);

            var placeholders = new Dictionary<string, string>
            {
                { "email", dto.Email },
                { "password", randomPassword },
                { "name", dto.FirstName },
                { "lastname", dto.LastName }
            };

            string emailBody = LoadEmailTemplate("welcome.html", placeholders);

            await _emailService.SendEmailAsync(dto.Email, "Witaj w PotoDocs 🚚", "Twoje dane do logowania", emailBody);

            user.PasswordHash = _hasher.HashPassword(user, randomPassword);
            await _context.Users.AddAsync(user);
        }
        else
        {
            _mapper.Map(dto, user);
        }

        user.Role = role;
        await _context.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(ChangePasswordDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email)
            ?? throw new BadRequestException("Nie znaleziono użytkownika");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.OldPassword);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Nieprawidłowe hasło");

        user.PasswordHash = _hasher.HashPassword(user, dto.NewPassword);

        var placeholders = new Dictionary<string, string>
        {
            { "email", user.Email },
            { "password", dto.NewPassword },
            { "name", user.FirstName },
            { "lastname", user.LastName }
        };

        string emailBody = LoadEmailTemplate("reset-password.html", placeholders);

        await _emailService.SendEmailAsync(user.Email, "Resetowanie hasła", "Twoje nowe hasło", emailBody);

        await _context.SaveChangesAsync();
    }

    public async Task GeneratePasswordAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email)
            ?? throw new BadRequestException("Nie znaleziono użytkownika");

        string randomPassword = GenerateRandomPassword(12);
        user.PasswordHash = _hasher.HashPassword(user, randomPassword);

        await _context.SaveChangesAsync();
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        var users = await _context.Users.Include(u => u.Role).ToListAsync();
        return _mapper.Map<List<UserDto>>(users);
    }

    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new UnauthorizedAccessException("Nie znaleziono użytkownika");

        return _mapper.Map<UserDto>(user);
    }

    public async Task DeleteAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    private string GenerateRandomPassword(int length)
    {
        const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        var chars = new char[length];

        for (int i = 0; i < length; i++)
        {
            chars[i] = validChars[random.Next(validChars.Length)];
        }

        return new string(chars);
    }

    private string LoadEmailTemplate(string templateName, Dictionary<string, string> placeholders)
    {
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "emails", templateName);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Nie znaleziono szablonu e-maila: {filePath}");

        string content = File.ReadAllText(filePath);

        foreach (var kv in placeholders)
        {
            content = content.Replace($"{{{kv.Key}}}", kv.Value);
        }

        return content;
    }
}
