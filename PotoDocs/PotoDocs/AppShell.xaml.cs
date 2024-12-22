using PotoDocs.Services;
using PotoDocs.View;

namespace PotoDocs;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(DetailsPage), typeof(DetailsPage));
        Routing.RegisterRoute(nameof(OrderFormPage), typeof(OrderFormPage));
        Routing.RegisterRoute(nameof(DriverFormPage), typeof(DriverFormPage));

        this.Navigated += OnShellNavigated;
    }

    private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
    {
        // Upewnij się, że LogoImage jest zdefiniowany
        if (LogoImage == null)
            return;

        // Sprawdzenie bieżącej strony
        var currentRoute = e?.Current?.Location?.OriginalString ?? string.Empty;

        // Ukryj logo na stronie "OrderFormPage"
        if (currentRoute.Contains("OrderFormPage"))
        {
            LogoImage.IsVisible = false;
        }
        else
        {
            LogoImage.IsVisible = true;
        }
    }
}
