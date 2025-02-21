using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using BlazorDownloadFile;
using Blazored.LocalStorage;
using Blazored.Modal;
using Blazored.Toast;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PotoDocs.Blazor;
using PotoDocs.Blazor.Models;
using PotoDocs.Blazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 🔹 **Dodanie autoryzacji**
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<JwtAuthenticationStateProvider>());

// 🔹 **Zewnętrzne usługi Blazored**
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredToast();
builder.Services.AddBlazorDownloadFile();
builder.Services.AddBlazoredModal();

using var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };

// 1️⃣ Pobranie ustawień konfiguracyjnych
var env = builder.HostEnvironment.Environment;
var configFile = env == "Production" ? "appsettings.Production.json" : "appsettings.json";

// Pobranie konfiguracji z odpowiedniego pliku
var configResponse = await httpClient.GetFromJsonAsync<Dictionary<string, object>>(configFile);

// 2️⃣ Pobranie sekcji "ApiSettings" i deserializacja
var apiSettings = JsonSerializer.Deserialize<ApiSettings>(configResponse["ApiSettings"].ToString() ?? "{}");

if (apiSettings is null || string.IsNullOrEmpty(apiSettings.BaseAddress))
{
    throw new Exception("Brak wartości `BaseAddress` w `appsettings.json`");
}


// 🔹 **Serwisy aplikacyjne**
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();

// 🔹 **HttpClient z poprawnym adresem API**
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiSettings.BaseAddress) });

// 🔹 **Uruchomienie aplikacji**
await builder.Build().RunAsync();
