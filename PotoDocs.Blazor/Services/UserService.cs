using System.Net.Http.Json;
using System.Text.Json;
using PotoDocs.Shared.Models;

namespace PotoDocs.Blazor.Services;

public interface IUserService
{
    Task RegisterAsync(UserDto dto);
    Task Update(UserDto dto);
    Task<IEnumerable<UserDto>> GetAll();
    Task GeneratePassword(Guid id);
    Task<UserDto> GetCurrentUser();
    Task ChangePassword(ChangePasswordDto dto);
    Task Delete(Guid id);
}

public class UserService(HttpClient http) : BaseService(http), IUserService
{

    public async Task RegisterAsync(UserDto dto)
    {
        await PostAsync("api/user/register", dto);
    }
    public async Task Update(UserDto dto)
    {
        await PutAsync("api/user", dto);
    }

    public async Task GeneratePassword(Guid id)
    {
        await PostAsync("api/user/generate-password", id);
    }

    public async Task<IEnumerable<UserDto>> GetAll()
    {
        return await GetAsync<IEnumerable<UserDto>>("api/user/all");
    }

    public async Task<UserDto> GetCurrentUser()
    {
        return await GetAsync<UserDto>("api/user/me");
    }

    public async Task ChangePassword(ChangePasswordDto dto)
    {
        await PostAsync("api/user/change-password", dto);
    }

    public async Task Delete(Guid id)
    {
        await DeleteAsync($"api/user/{id}");
    }
}