using BlazorDownloadFile;
using Blazored.LocalStorage;
using Blazored.Modal;
using Blazored.Toast;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PotoDocs.Blazor;
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

// 🔹 **Serwisy aplikacyjne**
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();

// 🔹 **HttpClient z poprawnym adresem API**
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://api.poto-express.com")
});

// 🔹 **Uruchomienie aplikacji**
await builder.Build().RunAsync();
