using PotoDocs.Services;
using PotoDocs.View;

namespace PotoDocs.ViewModel
{
    public partial class MainViewModel : BaseViewModel
    {
        public ObservableCollection<OrderDto> Orders { get; } = new();
        private readonly IOrderService _orderService;
        private readonly IConnectivity _connectivity;
        private readonly IUserService _userService;

        [ObservableProperty]
        bool isRefreshing;
        public MainViewModel(IOrderService orderService, IConnectivity connectivity, IUserService userService)
        {
            _orderService = orderService;
            _connectivity = connectivity;
            _userService = userService;
        }

        [RelayCommand]
        public async Task GetAll()
        {
            if (IsBusy)
                return;

            try
            {
                if (_connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    await Shell.Current.DisplayAlert("Brak połączenia!",
                        "Sprawdź połączenie z internetem i spróbuj ponownie.", "OK");
                    return;
                }

                IsBusy = true;

                var user = await _userService.GetUser();
                var orders = await _orderService.GetAll(1, 5, user.Email);

                if (orders != null)
                {
                    Orders.Clear();
                    foreach (var order in orders)
                    {
                        Orders.Add(order);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd pobierania ostatnich zleceń: {ex.Message}");
                await Shell.Current.DisplayAlert("Błąd!", ex.Message, "OK");
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
        async Task Add()
        {
            await Shell.Current.GoToAsync(nameof(OrderFormPage));
        }

        [RelayCommand]
        async Task Edit(OrderDto order)
        {
            if (order == null) return;

            await Shell.Current.GoToAsync(nameof(OrderFormPage), true, new Dictionary<string, object>
                {
                    {"OrderDto", order },
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
        [RelayCommand]
        async Task ShowMore()
        {
            if (IsBusy)
                return;

            try
            {
                if (_connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    await Shell.Current.DisplayAlert("Brak połączenia!",
                        "Sprawdź połączenie z internetem i spróbuj ponownie.", "OK");
                    return;
                }

                IsBusy = true;

                // Pobierz aktualną liczbę wyświetlonych zleceń
                int currentCount = Orders.Count;

                // Pobierz kolejne 5 zleceń
                var user = await _userService.GetUser();
                var newOrders = await _orderService.GetAll((currentCount / 5) + 1, 5, user.Email);

                if (newOrders != null && newOrders.Any())
                {
                    foreach (var order in newOrders)
                    {
                        Orders.Add(order); // Dodaj nowe zlecenia do istniejącej kolekcji
                    }
                }
                else
                {
                    await Shell.Current.DisplayAlert("Brak więcej zleceń",
                        "Wszystkie zlecenia zostały wyświetlone.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd podczas ładowania kolejnych zleceń: {ex.Message}");
                await Shell.Current.DisplayAlert("Błąd!", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
