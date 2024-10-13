using Microsoft.Maui.ApplicationModel;

namespace PotoDocs.ViewModel;

[QueryProperty(nameof(TransportOrderDto), "Transport order")]
public partial class TransportOrderDetailsViewModel : BaseViewModel
{
    IMap map;
    public TransportOrderDetailsViewModel(IMap map)
    {
        this.map = map;
    }

    [ObservableProperty]
    TransportOrderDto transportOrder;

    [RelayCommand]
    async Task OpenMap(Address address)
    {
        try
        {
            await map.OpenAsync(address.Latitude, address.Longitude, new MapLaunchOptions
            {
                Name = TransportOrder.CompanyName,
                NavigationMode = NavigationMode.None
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to launch maps: {ex.Message}");
            await Shell.Current.DisplayAlert("Error, no Maps app!", ex.Message, "OK");
        }
    }
    [RelayCommand]
    async Task GetDataFromAiAsync(Address address)
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