using PotoDocs.Services;
using PotoDocs.View;

namespace PotoDocs.ViewModel
{
    public partial class MainViewModel : BaseViewModel
    {
        public ObservableCollection<OrderDto> Orders { get; } = new();
        private readonly IOrderService _orderService;
        private readonly IConnectivity _connectivity;

        public MainViewModel(IOrderService orderService, IConnectivity connectivity)
        {
            Title = "Strona główna";
            _orderService = orderService;
            _connectivity = connectivity;

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
                if (_connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    await Shell.Current.DisplayAlert("Brak połączenia!",
                        "Sprawdź połączenie z internetem i spróbuj ponownie.", "OK");
                    return;
                }

                IsBusy = true;

                // Pobierz pierwsze 5 zamówień
                var orders = await _orderService.GetAll();

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
        async Task GoToDetails(OrderDto order)
        {
            if (order == null)
                return;

            await Shell.Current.GoToAsync(nameof(DetailsPage), true, new Dictionary<string, object>
        {
            {"OrderDto", order }
        });
        }
    }
}
