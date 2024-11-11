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
    }
}
