using Microsoft.AspNetCore.Identity;
using PotoDocs.API.Models;
using PotoDocs.Shared.Models;
using PotoDocs.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace PotoDocs.API.Services;

public interface IAccountService
{
    Task<LoginResponseDto> LoginAsync(LoginDto dto);
}

public class AccountService(PotodocsDbContext context, IPasswordHasher<User> hasher, ITokenService tokenService) : IAccountService
{
    private readonly PotodocsDbContext _context = context;
    private readonly IPasswordHasher<User> _hasher = hasher;
    private readonly ITokenService _tokenService = tokenService;

    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        const string invalidCredentialsMsg = "Nieprawidłowe hasło lub adres E-mail";

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == dto.Email) ?? throw new UnauthorizedAccessException(invalidCredentialsMsg);
        var verificationResult = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException(invalidCredentialsMsg);
        }

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _hasher.HashPassword(user, dto.Password);
            await _context.SaveChangesAsync();
        }

        var jwt = _tokenService.GenerateJWT(user);

        return new LoginResponseDto
        {
            Role = user.Role.Name,
            Token = jwt
        };
    }
}

