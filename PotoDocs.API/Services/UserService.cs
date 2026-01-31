using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PotoDocs.API.Entities;
using PotoDocs.API.Exceptions;
using PotoDocs.Shared.Models;
using System.Security.Cryptography;
using System.Text;

namespace PotoDocs.API.Services;

public interface IUserService
{
    Task RegisterAsync(UserDto dto);
    Task UpdateAsync(UserDto dto);
    Task ChangePasswordAsync(ChangePasswordDto dto);
    Task GeneratePasswordAsync(string email);
    Task<List<UserDto>> GetAllAsync();
    Task<UserDto> GetByIdAsync(Guid id);
    Task DeleteAsync(string email);

}

public class UserService(PotodocsDbContext context, IPasswordHasher<User> hasher, IMapper mapper, IEmailService emailService, IFileStorageService fileStorage) : IUserService
{
    private readonly PotodocsDbContext _context = context;
    private readonly IEmailService _emailService = emailService;
    private readonly IPasswordHasher<User> _hasher = hasher;
    private readonly IMapper _mapper = mapper;
    private readonly IFileStorageService _fileStorage = fileStorage;

    public async Task RegisterAsync(UserDto dto)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == dto.Role)
            ?? throw new BadRequestException($"Rola '{dto.Role}' nie istnieje.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        string? generatedPasswordToSend = null;

        if (user == null)
        {
            user = _mapper.Map<User>(dto);
            generatedPasswordToSend = GenerateRandomPassword(12);
            user.PasswordHash = _hasher.HashPassword(user, generatedPasswordToSend);

            await _context.Users.AddAsync(user);
        }
        else
        {
            _mapper.Map(dto, user);
        }

        user.Role = role;

        await _context.SaveChangesAsync();

        if (!string.IsNullOrEmpty(generatedPasswordToSend))
        {
            var placeholders = new Dictionary<string, string>
            {
                { "email", dto.Email },
                { "password", generatedPasswordToSend },
                { "name", dto.FirstName },
                { "lastname", dto.LastName }
            };

            string emailBody = await LoadEmailTemplateAsync("welcome.html", placeholders);
            await _emailService.SendEmailAsync(dto.Email, "Witaj w PotoDocs 🚚", emailBody, "Twoje dane do logowania");
        }
    }

    public async Task UpdateAsync(UserDto dto)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == dto.Role)
            ?? throw new BadRequestException($"Rola '{dto.Role}' nie istnieje.");

        var user = await _context.Users
            .SingleOrDefaultAsync(o => o.Id == dto.Id)
            ?? throw new KeyNotFoundException("Nie znaleziono użytkownika do aktualizacji.");

        _mapper.Map(dto, user);
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

        await _context.SaveChangesAsync();
    }

    public async Task GeneratePasswordAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email)
            ?? throw new BadRequestException("Nie znaleziono użytkownika");

        string randomPassword = GenerateRandomPassword(12);
        user.PasswordHash = _hasher.HashPassword(user, randomPassword);

        await _context.SaveChangesAsync();

        var placeholders = new Dictionary<string, string>
        {
            { "email", user.Email },
            { "password", randomPassword },
            { "name", user.FirstName },
            { "lastname", user.LastName }
        };

        string emailBody = await LoadEmailTemplateAsync("reset-password.html", placeholders);
        await _emailService.SendEmailAsync(user.Email, "Resetowanie hasła", emailBody, $"Twoje nowe hasło: {randomPassword}");
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        var users = await _context.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<List<UserDto>>(users);
    }

    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new KeyNotFoundException("Nie znaleziono użytkownika");

        return _mapper.Map<UserDto>(user);
    }

    public async Task DeleteAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    private async Task<string> LoadEmailTemplateAsync(string templateName, Dictionary<string, string> placeholders)
    {
        var (bytes, _) = await _fileStorage.GetFileAsync(FileType.EmailTemplate, templateName);
        string content = Encoding.UTF8.GetString(bytes);

        foreach (var kv in placeholders)
        {
            content = content.Replace($"{{{kv.Key}}}", kv.Value);
        }

        return content;
    }

    private static string GenerateRandomPassword(int length)
    {
        const string validChars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%^&*";

        return string.Create(length, validChars, (span, chars) =>
        {
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
            }
        });
    }
}