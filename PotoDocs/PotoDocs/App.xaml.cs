using PotoDocs.Converters;
using PotoDocs.Services;
using PotoDocs.View;

namespace PotoDocs;

public partial class App : Application
{
    public App()
    {

        InitializeComponent();

        Resources.Add("OrderStatusConverter", new OrderStatusConverter());
        Resources.Add("OrderStatusColorConverter", new OrderStatusColorConverter());
        Application.Current.UserAppTheme = AppTheme.Light;
        MainPage = new AppShell();
    }
}
