using BlazorDownloadFile;
using Blazored.LocalStorage;
using Blazored.Modal;
using FluentValidation;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using PotoDocs.Blazor;
using PotoDocs.Blazor.Helpers;
using PotoDocs.Blazor.Models;
using PotoDocs.Blazor.Services;
using PotoDocs.Blazor.Services.Strategies;
using PotoDocs.Shared.Models;
using System.Net.Http.Json;
using System.Text.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// --- KONFIGURACJA (bez zmian) ---
using var httpClientConfig = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var env = builder.HostEnvironment.Environment;
var configFile = env == "Development" ? "appsettings.Development.json" : "appsettings.json";
var configResponse = await httpClientConfig.GetFromJsonAsync<Dictionary<string, object>>(configFile);
var apiSettings = JsonSerializer.Deserialize<ApiSettings>(configResponse?["ApiSettings"].ToString() ?? "{}");

if (apiSettings is null || string.IsNullOrEmpty(apiSettings.BaseAddress))
{
    throw new Exception("Brak wartości `BaseAddress` w `appsettings.json`");
}

// 🔹 **1. Autoryzacja i Interceptor**
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddTransient<HttpInterceptorService>(); // Musi być Transient

// 🔹 **2. Rejestracja HTTP Clientów**

// A. Klient PUBLICZNY (do logowania) - Bez interceptora
builder.Services.AddHttpClient("PotoDocs.Public", client =>
{
    client.BaseAddress = new Uri(apiSettings.BaseAddress);
});

// B. Klient PRYWATNY (do API) - Z interceptorem
// Interceptor automatycznie doda Token i obsłuży 401
builder.Services.AddHttpClient("PotoDocs.API", client =>
{
    client.BaseAddress = new Uri(apiSettings.BaseAddress);
})
.AddHttpMessageHandler<HttpInterceptorService>();

// C. Domyślny HttpClient dla serwisów (OrderService, InvoiceService itp.)
// Wstrzykując HttpClient w serwisach, dostaniesz ten "PotoDocs.API" z interceptorem
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("PotoDocs.API"));


// 🔹 **3. Pozostałe Serwisy**
builder.Services.AddBlazorDownloadFile();
builder.Services.AddBlazoredModal();
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;
    // ... twoja konfiguracja snackbara ...
});

builder.Services.AddScoped<IFileDownloadHelper, FileDownloadHelper>();
builder.Services.AddScoped<IAuthService, AuthService>(); // Teraz używa Factory
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IDownloadsService, DownloadsService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

builder.Services.AddScoped<IValidator<LoginDto>, LoginDtoValidator>();

builder.Services.AddScoped<IInvoiceActionStrategy, OriginalInvoiceStrategy>();
builder.Services.AddScoped<IInvoiceActionStrategy, CorrectionStrategy>();
builder.Services.AddScoped<InvoiceStrategyFactory>();

await builder.Build().RunAsync();