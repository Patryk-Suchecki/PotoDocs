using PotoDocs.Services;
using PotoDocs.View;

namespace PotoDocs.ViewModel;

public partial class OrdersViewModel : BaseViewModel
{
    public ObservableCollection<OrderDto> Orders { get; } = new();
    IOrderService _orderService;
    IConnectivity _connectivity;
    public OrdersViewModel(IOrderService orderService, IConnectivity connectivity)
    {
        _orderService = orderService;
        _connectivity = connectivity;
        GetAll();
    }

    [ObservableProperty]
    bool isRefreshing;

    [RelayCommand]
    async Task GetAll()
    {
        if (IsBusy) return;

        try
        {
            if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await Shell.Current.DisplayAlert("No connectivity!",
                    $"Please check your internet connection and try again.", "OK");
                return;
            }
            IsRefreshing = true;
            IsBusy = true;
            var orders = await _orderService.GetAll();

            // Pobranie danych z API
            var response = await orderService.GetAll(null, currentPage, pageSize);

            if (response != null)
            {
                if (currentPage == 1) // Jeśli to pierwsza strona, wyczyść listę
                    Orders.Clear();

                foreach (var order in response.Items)
                {
                    Orders.Add(order);
                }

                // Zaktualizuj dane paginacji
                totalPages = response.TotalPages;
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Błąd!", "Nie udało się pobrać zleceń:" + ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    async Task Details(OrderDto order)
    {
        if (order == null) return;

        await Shell.Current.GoToAsync(nameof(DetailsPage), true, new Dictionary<string, object>
        {
            {"OrderDto", order }
        });
    }
    [RelayCommand]
    async Task Create()
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
                OrderDto order = await _orderService.Create(result.FullPath);

                await Shell.Current.GoToAsync(nameof(OrderFormPage), true, new Dictionary<string, object>
                {
                    {"OrderDto", order },
                    {"title", "Dodaj nowe zlecenie" },
                    { "InvoiceNumber", order.InvoiceNumber}
                });
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Błąd!", "Nie udało się utworzyć zlecenia:" + ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
    [RelayCommand]
    async Task Edit(OrderDto order)
    {
        if (order == null) return;

        await Shell.Current.GoToAsync(nameof(OrderFormPage), true, new Dictionary<string, object>
                {
                    {"OrderDto", order },
                    { "title", "Edytuj zlecenie" },
                    { "InvoiceNumber", order.InvoiceNumber}
                });

    }
    [RelayCommand]
    async Task Delete(OrderDto order)
    {
        if (order == null)
            return;
        await _orderService.Delete(order.InvoiceNumber);
        GetAll();
    }
    
    [RelayCommand]
    async Task DownloadInvoice(OrderDto order)
    {
        if (order == null)
            return;
        IsBusy = true;
        string outputPath = await _orderService.DownloadInvoice(order.InvoiceNumber);
#if WINDOWS
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = outputPath,
                UseShellExecute = true
            }
        };
        process.Start();
        process.Close();
#else
        await Share.RequestAsync(new ShareFileRequest
        {
            Title = "Zapisz pdf",
            File = new ShareFile(outputPath)
        });

#endif
        IsBusy = false;
    }
}

