using PotoDocs.Services;
using PotoDocs.View;
using System.Text;
using PdfPigPage = UglyToad.PdfPig.Content.Page;

namespace PotoDocs.ViewModel;

public partial class TransportOrdersViewModel : BaseViewModel
{
    public ObservableCollection<TransportOrderDto> TransportOrders { get; } = new();
    TransportOrderService transportOrderService;
    IConnectivity connectivity;
    IGeolocation geolocation;
    public TransportOrdersViewModel(TransportOrderService transportOrderService, IConnectivity connectivity, IGeolocation geolocation)
    {
        Title = "Zlecenia";
        this.transportOrderService = transportOrderService;
        this.connectivity = connectivity;
        this.geolocation = geolocation;
        GetTransportOrdersAsync();
    }

    [ObservableProperty]
    bool isRefreshing;

    [RelayCommand]
    async Task GetTransportOrdersAsync()
    {
        if (IsBusy)
            return;

        try
        {
            if (connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await Shell.Current.DisplayAlert("No connectivity!",
                    $"Please check internet and try again.", "OK");
                return;
            }

            IsBusy = true;
            var transportOrders = await transportOrderService.GetTransportOrders();

            if (TransportOrders.Count != 0)
                TransportOrders.Clear();

            foreach (var transportOrder in transportOrders)
                TransportOrders.Add(transportOrder);

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to get monkeys: {ex.Message}");
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }

    }

    [RelayCommand]
    async Task GoToDetails(TransportOrderDto transportOrder)
    {
        if (transportOrder == null)
            return;

        await Shell.Current.GoToAsync(nameof(DetailsPage), true, new Dictionary<string, object>
        {
            {"Transport order", transportOrder }
        });
    }
    [RelayCommand]
    async Task GoToNewOrder()
    {
        try
        {
            var pdfFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, new[] { "com.adobe.pdf" } },
                { DevicePlatform.Android, new[] { "application/pdf" } },
                { DevicePlatform.WinUI, new[] { ".pdf" } },
                { DevicePlatform.MacCatalyst, new[] { "pdf" } }
            });

            var pickOptions = new PickOptions
            {
                PickerTitle = "Wybierz plik PDF",
                FileTypes = pdfFileType
            };

            var result = await FilePicker.Default.PickAsync(pickOptions);
            if (result != null && result.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                IsBusy = true;
                TransportOrderDto transportOrder = await transportOrderService.UploadFile(result.FullPath);
                transportOrder.PDFUrl = result.FullPath;

                await Shell.Current.GoToAsync(nameof(TransportOrderFormPage), true, new Dictionary<string, object>
                {
                    {"Transport order", transportOrder },
                    {"title", "Dodaj nowe zlecenie" }
                });
            }
            else
            {
                Debug.WriteLine("Wybrany plik nie jest plikiem PDF.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Błąd:  {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
    [RelayCommand]
    async Task GoToEditOrder(TransportOrderDto transportOrder)
    {
        if (transportOrder == null)
            return;

        await Shell.Current.GoToAsync(nameof(TransportOrderFormPage), true, new Dictionary<string, object>
                {
                    {"Transport order", transportOrder },
                    { "title", "Edytuj zlecenie" }
                });

    }
    [RelayCommand]
    async Task DeleteOrder(TransportOrderDto transportOrder)
    {
        if (transportOrder == null)
            return;
    }
    
    private string ExtractTextFromPdf(Stream pdfStream)
    {
        StringBuilder textBuilder = new StringBuilder();
        var tempFile = Path.GetTempFileName();

        using (var fileStream = File.Create(tempFile))
        {
            pdfStream.CopyTo(fileStream);
        }

        using (var document = UglyToad.PdfPig.PdfDocument.Open(tempFile))
        {
            foreach (PdfPigPage page in document.GetPages())
            {
                textBuilder.Append(page.Text);
            }
        }

        File.Delete(tempFile);
        return textBuilder.ToString();
    }
}

