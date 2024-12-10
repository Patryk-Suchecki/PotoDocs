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
    ApiResponse<string> RegisterUser(UserDto dto);
    ApiResponse<string> ChangePassword(ChangePasswordDto dto);
    ApiResponse<string> GeneratePassword(string email);
    ApiResponse<List<UserDto>> GetAll();
    ApiResponse<UserDto> GetUser(int id);
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
    public ApiResponse<string> RegisterUser(UserDto dto)
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
            _emailService.SendEmail(dto.Email, "Rejestracja PotoDocs", $"Witaj, Twoje dane do logowania to:\nEmail: {dto.Email}\nHasło: {randomPassword}", $@"
            <html>
                <body>
                    <h1>Witaj!</h1>
                    <p>Twoje dane do logowania:</p>
                    <p><b>Email:</b> {dto.Email}</p>
                    <p><b>Hasło:</b> {randomPassword}</p>
                    <p>Prosimy o zachowanie tych informacji w bezpiecznym miejscu.</p>
                </body>
            </html>");
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

        var newPasswordHash = _hasher.HashPassword(user, dto.NewPassword);
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
        _emailService.SendEmail(email, "Rejestracja PotoDocs", $"Witaj, Twoje dane do logowania to:\nEmail: {email}\nHasło: {randomPassword}", $@"
        <html>
            <body>
                <h1>Witaj!</h1>
                <p>Twoje dane do logowania:</p>
                <p><b>Email:</b> {email}</p>
                <p><b>Hasło:</b> {randomPassword}</p>
                <p>Prosimy o zachowanie tych informacji w bezpiecznym miejscu.</p>
            </body>
        </html>");
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

    public ApiResponse<UserDto> GetUser(int id)
    {
        var user = _context.Users.Include(u => u.Role).FirstOrDefault(u => u.Id == id);
        if (user == null) return ApiResponse<UserDto>.Failure("Nie znaleziono użytkownika", HttpStatusCode.Unauthorized);

        var dto = _mapper.Map<UserDto>(user);

        return ApiResponse<UserDto>.Success(dto);
    }
}

