using PotoDocs.Services;
using PotoDocs.View;

namespace PotoDocs.ViewModel;

public partial class DriversViewModel : BaseViewModel
{
    public ObservableCollection<DriverDto> Drivers { get; } = new();
    DriverService driverService;
    public DriversViewModel(DriverService driverService)
    {
        this.driverService = driverService;
        GetDriversAsync();
    }

    [ObservableProperty]
    bool isRefreshing;

    [RelayCommand]
    async Task GetDriversAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            var drivers = await driverService.GetDrivers();

            if (Drivers.Count != 0)
                Drivers.Clear();

            foreach (var driver in drivers)
                Drivers.Add(driver);

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to get drivers: {ex.Message}");
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }

    }
    [RelayCommand]
    async Task GoToNewDriver()
    {
        await Shell.Current.GoToAsync(nameof(DriverFormPage), true, new Dictionary<string, object>
        {
            {"Driver", new DriverDto() },
            {"title", "Dodaj kierowce" }
        });
    }
    [RelayCommand]
    async Task GoToEditDriver(DriverDto driver)
    {
        if (driver == null)
            return;

        await Shell.Current.GoToAsync(nameof(DriverFormPage), true, new Dictionary<string, object>
        {
            {"Driver", driver },
            {"title", "Edytuj kierowce" }
        });

    }
    [RelayCommand]
    async Task DeleteDriver(DriverDto transportOrder)
    {
        if (transportOrder == null)
            return;
    }
}