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
    void RegisterUser(UserDto dto);
    void ChangePassword(ChangePasswordDto dto);
    ApiResponse<List<UserDto>> GetAll();
    Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default);
}

public class AccountService : IAccountService
{
    private readonly AuthenticationSettings _authSettings;
    private readonly PotodocsDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IMapper _mapper;

    public AccountService(PotodocsDbContext context, IPasswordHasher<User> hasher, AuthenticationSettings authenticationSettings, IMapper mapper, ITokenService tokenService)
    {
        _authSettings = authenticationSettings;
        _context = context;
        _hasher = hasher;
        _mapper = mapper;
        _tokenService = tokenService;
    }
    public void RegisterUser(UserDto dto)
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
    public ApiResponse<List<UserDto>> GetAll()
    {
        var users = _context.Users.Include(u => u.Role).ToList();
        var usersDto = _mapper.Map<List<UserDto>>(users);
        return ApiResponse<List<UserDto>>.Success(usersDto);
    }
    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        var user = _context.Users.Include(u => u.Role).FirstOrDefault(u => u.Email == dto.Username);

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
            Role = user.Role,
            Token = jwt
        };
        return ApiResponse<LoginResponseDto>.Success(authResponse);
    }
}
