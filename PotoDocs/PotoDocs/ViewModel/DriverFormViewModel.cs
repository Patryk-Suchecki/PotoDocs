using PotoDocs.Services;
using PotoDocs.View;

namespace PotoDocs.ViewModel;

[QueryProperty(nameof(PageTitle), "title")]
[QueryProperty(nameof(RegisterUserDto), "Driver")]
public partial class DriverFormViewModel : BaseViewModel
{
    [ObservableProperty]
    RegisterUserDto driverDto;
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
    async Task SaveDriver(RegisterUserDto driver)
    {
        if (driver == null)
            return;
    }
    [RelayCommand]
    async Task GenerateNewPassword(RegisterUserDto driver)
    {
        if (driver == null)
            return;
    }
}

