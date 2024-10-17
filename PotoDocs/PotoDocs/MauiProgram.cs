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
            });

        builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
        builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);
        builder.Services.AddSingleton<IMap>(Map.Default);

        builder.Services.AddHttpClient<AuthService>();
        builder.Services.AddTransient<AuthHttpClientHandler>();

        builder.Services.AddHttpClient<OrderService>()
            .AddHttpMessageHandler<AuthHttpClientHandler>();

        builder.Services.AddHttpClient<DriverService>()
            .AddHttpMessageHandler<AuthHttpClientHandler>();

        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<LoginPage>();

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
