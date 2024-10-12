using PotoDocs.Services;
using PotoDocs.View;

namespace PotoDocs.ViewModel;

[QueryProperty(nameof(PageTitle), "title")]
[QueryProperty(nameof(DriverDto), "Driver")]
public partial class DriverFormViewModel : BaseViewModel
{
    [ObservableProperty]
    DriverDto driverDto;
    DriverService driverService;

    string pageTitle;
    public string PageTitle
    {
        get => pageTitle;
        set
        {
            pageTitle = value;
            Title = pageTitle;
        }
    }
    public DriverFormViewModel(DriverService driverService)
    {
        this.driverService = driverService;
    }
    [RelayCommand]
    async Task SaveDriver(DriverDto driver)
    {
        if (driver == null)
            return;
    }
    [RelayCommand]
    async Task GenerateNewPassword(DriverDto driver)
    {
        if (driver == null)
            return;
    }
}

