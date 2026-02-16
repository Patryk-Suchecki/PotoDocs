using System.Net;
using System.Net.Http.Headers;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace PotoDocs.Blazor.Services;

public class HttpInterceptorService(
    AuthenticationStateProvider authStateProvider,
    ILocalStorageService localStorage,
    NavigationManager navigationManager) : DelegatingHandler
{
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;
    private readonly ILocalStorageService _localStorage = localStorage;
    private readonly NavigationManager _navigationManager = navigationManager;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _localStorage.GetItemAsStringAsync("authToken");

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            if (!_navigationManager.Uri.Contains("/logowanie"))
            {
                if (_authStateProvider is JwtAuthenticationStateProvider customProvider)
                {
                    await customProvider.Logout();
                }
            }
        }

        return response;
    }
}