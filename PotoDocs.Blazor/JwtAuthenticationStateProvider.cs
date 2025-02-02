using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Net.Http.Headers;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;

namespace PotoDocs.Blazor;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _httpClient;
    private readonly NavigationManager _navigation;
    private const string AuthTokenKey = "authToken";

    public JwtAuthenticationStateProvider(ILocalStorageService localStorage, HttpClient httpClient, NavigationManager navigation)
    {
        _localStorage = localStorage;
        _httpClient = httpClient;
        _navigation = navigation;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsStringAsync(AuthTokenKey);
        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                await Logout();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var identity = new ClaimsIdentity(jwtToken.Claims, "jwtAuthType");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
        catch
        {
            await Logout();
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public async Task Login(string token)
    {
        await _localStorage.SetItemAsStringAsync(AuthTokenKey, token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task Logout()
    {
        await _localStorage.RemoveItemAsync(AuthTokenKey);
        _httpClient.DefaultRequestHeaders.Authorization = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        _navigation.NavigateTo("/login", true); // Tu jest lepsze miejsce na przekierowanie
    }

    public async Task<string?> GetToken()
    {
        var token = await _localStorage.GetItemAsStringAsync(AuthTokenKey);
        return string.IsNullOrWhiteSpace(token) ? null : token;
    }

    public async Task<bool> IsUserAuthenticated()
    {
        var token = await _localStorage.GetItemAsStringAsync(AuthTokenKey);
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        return jwtToken.ValidTo > DateTime.UtcNow;
    }
}
