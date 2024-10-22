using Microsoft.Maui.ApplicationModel;

namespace PotoDocs.ViewModel;

[QueryProperty(nameof(OrderDto), "OrderDto")]
public partial class OrderDetailsViewModel : BaseViewModel
{
    IMap map;
    public OrderDetailsViewModel(IMap map)
    {
        this.map = map;
    }

    [ObservableProperty]
    OrderDto orderDto;

/*    [RelayCommand]
    async Task OpenMap(double latitude, double longitude)
    {
        try
        {
            await map.OpenAsync(latitude, longitude, new MapLaunchOptions
            {
                Name = Order.CompanyName,
                NavigationMode = NavigationMode.None
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to launch maps: {ex.Message}");
            await Shell.Current.DisplayAlert("Error, no Maps app!", ex.Message, "OK");
        }
    }*/
    [RelayCommand]
    async Task GetDataFromAiAsync()
    {
        try
        {
        }
        catch (Exception ex)
        {
        }
    }
    [RelayCommand]
    async Task OpenPdfCommand(String url)
    {
        try
        {
        }
        catch (Exception ex)
        {
        }
    }
}