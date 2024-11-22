using PotoDocs.Services;
using PotoDocs.View;

namespace PotoDocs;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            System.Diagnostics.Debug.WriteLine($"Unhandled exception: {e.ExceptionObject}");
        };
    }
}
