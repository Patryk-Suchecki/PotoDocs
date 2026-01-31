using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using PotoDocs.API;
using PotoDocs.API.Entities;
using PotoDocs.API.Models.Validators;
using PotoDocs.API.Options;
using PotoDocs.API.Services;
using PotoDocs.Shared.Models;
using QuestPDF.Drawing;
using QuestPDF.Infrastructure;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Community;

using var tahomaRegular = File.OpenRead("wwwroot/fonts/tahoma.ttf");
FontManager.RegisterFontWithCustomName("Tahoma", tahomaRegular);

using var tahomaBold = File.OpenRead("wwwroot/fonts/tahomabd.ttf");
FontManager.RegisterFontWithCustomName("Tahoma-Bold", tahomaBold);

builder.Services.Configure<EmailServiceOptions>(builder.Configuration.GetSection("EmailService"));
builder.Services.Configure<OpenAIOptions>(builder.Configuration.GetSection("OpenAI"));
builder.Services.Configure<OrganizationSettings>(builder.Configuration.GetSection("OrganizationSettings"));
builder.Services.Configure<AuthenticationSettings>(builder.Configuration.GetSection("Authentication"));
builder.Services.AddAzureClients(clientBuilder =>
{
    var emailConfig = builder.Configuration.GetSection("EmailService");
    var connectionString = emailConfig["ConnectionString"];

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("EmailService: ConnectionString is not configured in appsettings.json section 'EmailService'");
    }

    clientBuilder.AddEmailClient(connectionString);
});

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            TokenService.GetTokenValidationParameters(builder.Configuration);
    });
builder.Services.AddTransient<ITokenService, TokenService>()
                .AddTransient<IAccountService, AccountService>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddScoped<ErrorHandlingMiddleware>();
var provider = builder.Configuration.GetValue<string>("DatabaseProvider");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<PotodocsDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<DBSeeder>();
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration["AllowedOrigins"]?.Split(';', StringSplitOptions.RemoveEmptyEntries);
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins(allowedOrigins ?? [])
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("Content-Disposition");
    });
});

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IValidator<UserDto>, RegisterUserDtoValidator>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IInvoiceNumberGenerator, InvoiceNumberGenerator>();
builder.Services.AddScoped<IDownloadService, DownloadService>();
builder.Services.AddScoped<IInvoicePdfGenerator, InvoicePdfGenerator>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IOrderDocumentSender, OrderDocumentSender>();

builder.Services.AddHttpClient<IOpenAIService, OpenAIService>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<OpenAIOptions>>().Value;
    client.BaseAddress = new Uri("https://api.openai.com/v1/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.APIKey);
});
builder.Services.AddHttpClient<IEuroRateService, NbpEuroRateService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DBSeeder>();
    seeder.Seed();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("AllowBlazorClient");
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
