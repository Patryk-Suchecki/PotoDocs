using PotoDocs.Services;
using PotoDocs.View;
using System.Text;
using PdfPigPage = UglyToad.PdfPig.Content.Page;

namespace PotoDocs.ViewModel;

public partial class OrdersViewModel : BaseViewModel
{
    public ObservableCollection<OrderDto> Orders { get; } = new();
    OrderService orderService;
    IConnectivity connectivity;
    IGeolocation geolocation;
    public OrdersViewModel(OrderService orderService, IConnectivity connectivity, IGeolocation geolocation)
    {
        Title = "Zlecenia";
        this.orderService = orderService;
        this.connectivity = connectivity;
        this.geolocation = geolocation;
        GetOrdersAsync();
    }

    [ObservableProperty]
    bool isRefreshing;

    [RelayCommand]
    async Task GetOrdersAsync()
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
            var orders = await orderService.GetOrders();

            if (Orders.Count != 0)
                Orders.Clear();

            foreach (var order in orders)
                Orders.Add(order);

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to get orders: {ex.Message}");
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }

    }

    [RelayCommand]
    async Task GoToDetails(OrderDto order)
    {
        if (order == null)
            return;

        await Shell.Current.GoToAsync(nameof(DetailsPage), true, new Dictionary<string, object>
        {
            {"OrderDto", order }
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
                OrderDto order = await orderService.UploadFile(result.FullPath);
                order.PDFUrl = result.FullPath;

                await Shell.Current.GoToAsync(nameof(OrderFormPage), true, new Dictionary<string, object>
                {
                    {"OrderDto", order },
                    {"title", "Dodaj nowe zlecenie" },
                    { "InvoiceNumber", order.InvoiceNumber}
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
    async Task GoToEditOrder(OrderDto order)
    {
        if (order == null)
            return;

        await Shell.Current.GoToAsync(nameof(OrderFormPage), true, new Dictionary<string, object>
                {
                    {"OrderDto", order },
                    { "title", "Edytuj zlecenie" },
                    { "InvoiceNumber", order.InvoiceNumber}
                });

    }
    [RelayCommand]
    async Task DeleteOrder(OrderDto order)
    {
        if (order == null)
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

