using System.Net.Http.Headers;
using PotoDocs.Services;

namespace PotoDocs;

public class AuthHttpClientHandler : DelegatingHandler
{
    private readonly AuthService _authService;

    public AuthHttpClientHandler(AuthService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var user = await _authService.GetAuthenticatedUserAsync();
        if (!string.IsNullOrEmpty(user.Token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
