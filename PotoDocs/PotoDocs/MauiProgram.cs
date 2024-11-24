using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PotoDocs.Services;
using PotoDocs.View;

namespace PotoDocs;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Roboto-Bold.ttf", "RobotoBold");
                fonts.AddFont("Roboto-Italic.ttf", "RobotoItalic");
                fonts.AddFont("Roboto-Medium.ttf", "RobotoMedium");
                fonts.AddFont("Roboto-Regular.ttf", "RobotoItalic");
            });
//        builder.ConfigureMauiHandlers(handlers =>
//        {
//#if WINDOWS
//    handlers.AddHandler(typeof(SwipeView), typeof(CustomSwipeViewHandler));
//#endif
//        });
        builder.Services.AddCustomApiHttpClient();
        builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
        builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);
        builder.Services.AddSingleton<IMap>(Map.Default);

        builder.Services.AddSingleton<IAuthService, AuthService>();

        builder.Services.AddTransient<OrderService>();

        builder.Services.AddTransient<UserService>();

        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<LoginPage>();

        builder.Services.AddSingleton<OrdersViewModel>();
        builder.Services.AddSingleton<OrdersPage>();

        builder.Services.AddTransient<OrderDetailsViewModel>();
        builder.Services.AddTransient<DetailsPage>();

        builder.Services.AddTransient<OrderFormViewModel>();
        builder.Services.AddTransient<OrderFormPage>();

        builder.Services.AddTransient<DownloadViewModel>();
        builder.Services.AddTransient<DownloadPage>();


        builder.Services.AddTransient<DriverFormViewModel>();
        builder.Services.AddTransient<DriverFormPage>();

        builder.Services.AddTransient<DriversViewModel>();
        builder.Services.AddTransient<DriversPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
public static class MauiProgramExtensions
{
    public static IServiceCollection AddCustomApiHttpClient(this IServiceCollection services)
    {
#if WINDOWS
        services.AddHttpClient(AppConstants.HttpClientName, httpClient =>
        {
            var baseAddress = "https://localhost:7157";

            httpClient.BaseAddress = new Uri(baseAddress);
        });
#else
        // Rejestracja platformowego handlera HTTP
        services.AddSingleton<IPlatformHttpMessageHandler>(sp =>
        {
#if ANDROID
            return new AndroidHttpMessageHandler();
#elif IOS
            return new IosHttpMessageHandler();
#else
            return null;
#endif
        });

        // Rejestracja HttpClient z platformowym handlerem
        services.AddHttpClient(AppConstants.HttpClientName, httpClient =>
        {
            var baseAddress = DeviceInfo.Platform == DevicePlatform.Android
                ? "https://10.0.2.2:7157" // Android (emulator)
                : "https://localhost:7157"; // Inne platformy

            httpClient.BaseAddress = new Uri(baseAddress);
        })
        .ConfigurePrimaryHttpMessageHandler(sp =>
        {
            // Użyj platformowego handlera jako PrimaryHandler
            var platformHttpMessageHandler = sp.GetRequiredService<IPlatformHttpMessageHandler>();
            return platformHttpMessageHandler.GetHttpMessageHandler();
        });
#endif


        return services;
    }
}
