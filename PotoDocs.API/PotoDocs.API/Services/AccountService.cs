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
using AutoMapper;
using System.Net;

namespace PotoDocs.API.Services;

public interface IAccountService
{
    ApiResponse<string> RegisterUser(UserDto dto);
    ApiResponse<string> ChangePassword(ChangePasswordDto dto);
    ApiResponse<string> GeneratePassword(string email);
    ApiResponse<List<UserDto>> GetAll();
    Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    ApiResponse<List<string>> GetRoles();
}

public class AccountService : IAccountService
{
    private readonly AuthenticationSettings _authSettings;
    private readonly PotodocsDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IMapper _mapper;

    public AccountService(PotodocsDbContext context, IPasswordHasher<User> hasher, AuthenticationSettings authenticationSettings, IMapper mapper, ITokenService tokenService, IEmailService emailService)
    {
        _authSettings = authenticationSettings;
        _context = context;
        _hasher = hasher;
        _mapper = mapper;
        _tokenService = tokenService;
        _emailService = emailService;
    }
    public ApiResponse<string> RegisterUser(UserDto dto)
    {
        var role = _context.Roles.FirstOrDefault(r => r.Name == dto.Role);

        if (role == null)
        {
            return ApiResponse<string>.Failure($"Rola '{dto.Role}' nie istnieje.", HttpStatusCode.BadRequest);
        }

        var user = _mapper.Map<User>(dto);
        user.Role = role;
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
    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var user = _context.Users.Include(u => u.Role).FirstOrDefault(u => u.Email == dto.Email);

        if (user is null)
        {
            return ApiResponse<LoginResponseDto>.Failure("Użytkownik lub hasło są niepoprawne", HttpStatusCode.Unauthorized);
        }

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return ApiResponse<LoginResponseDto>.Failure("Użytkownik lub hasło są niepoprawne", HttpStatusCode.Unauthorized);
        }

        var jwt = _tokenService.GenerateJWT(user);
        var authResponse = new LoginResponseDto
        {
            Role = user.Role.Name,
            Token = jwt
        };
        return ApiResponse<LoginResponseDto>.Success(authResponse);
    }

    public ApiResponse<List<string>> GetRoles()
    {
        var roleNames = _context.Roles.Select(role => role.Name).ToList();
        return ApiResponse<List<string>>.Success(roleNames);
    }
}
