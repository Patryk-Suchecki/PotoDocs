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

        MainPage = new AppShell();
    }
}
