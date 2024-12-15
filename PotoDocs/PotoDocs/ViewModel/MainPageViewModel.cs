using PotoDocs.Services;
using PotoDocs.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PotoDocs.ViewModel
{
    public partial class MainPageViewModel : BaseViewModel
    {
        public ObservableCollection<OrderDto> RecentOrders { get; } = new();
        private readonly OrderService orderService;
        private readonly IConnectivity connectivity;

        public MainPageViewModel(OrderService orderService, IConnectivity connectivity)
        {
            Title = "Strona główna";
            this.orderService = orderService;
            this.connectivity = connectivity;

            GetRecentOrdersAsync();
        }

        [ObservableProperty]
        bool isRefreshing;

        [RelayCommand]
        async Task GetRecentOrdersAsync()
        {
            if (IsBusy)
                return;

            try
            {
                if (connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    await Shell.Current.DisplayAlert("Brak połączenia!",
                        "Sprawdź połączenie z internetem i spróbuj ponownie.", "OK");
                    return;
                }

                IsBusy = true;

                // Pobierz pierwsze 5 zamówień
                var response = await orderService.GetAll(null, 1, 5);

                if (response != null)
                {
                    RecentOrders.Clear();
                    foreach (var order in response.Items)
                    {
                        RecentOrders.Add(order);
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
