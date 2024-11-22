using PotoDocs.Services;
using PotoDocs.View;

namespace PotoDocs;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }
}
