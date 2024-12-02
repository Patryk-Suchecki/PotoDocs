using PotoDocs.Services;

namespace PotoDocs.ViewModel;

public partial class DownloadViewModel : BaseViewModel
{
    IOrderService _orderService;
    public List<string> Months { get; } = new List<string>
        {
            "Styczeń", "Luty", "Marzec", "Kwiecień", "Maj", "Czerwiec",
            "Lipiec", "Sierpień", "Wrzesień", "Październik", "Listopad", "Grudzień"
        };
    public List<int> Years { get; } = new List<int> { 2023, 2024};

    [ObservableProperty]
    DownloadDto downloadDto;

    public DownloadViewModel(IOrderService orderService)
    {
        _orderService = orderService;

        downloadDto = new DownloadDto { Month = DateTime.Now.Month, Year = DateTime.Now.Year};
    }

    [RelayCommand]
    async Task GetOrdersAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            string outputPath = await _orderService.DownloadInvoices(downloadDto);
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Zapisz faktury",
                File = new ShareFile(outputPath)
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to download: {ex.Message}");
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
