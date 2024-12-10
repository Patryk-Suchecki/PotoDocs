using Microsoft.AspNetCore.Identity;
using PotoDocs.API.Models;
using PotoDocs.Shared.Models;
using PotoDocs.API.Entities;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace PotoDocs.API.Services;

public interface IAccountService
{
    Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
}

public class AccountService : IAccountService
{
    private readonly PotodocsDbContext _context;
    private readonly IPasswordHasher<User> _hasher;
    private readonly ITokenService _tokenService;

    public AccountService(PotodocsDbContext context, IPasswordHasher<User> hasher, ITokenService tokenService)
    {
        _context = context;
        _hasher = hasher;
        _tokenService = tokenService;
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
}
