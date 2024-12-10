

using PotoDocs.Services;

namespace PotoDocs.ViewModel;

public partial class MainViewModel : BaseViewModel
{
    public ObservableCollection<OrderDto> Orders { get; } = new();
    [ObservableProperty]
    bool isRefreshing;
    [ObservableProperty]
    private UserDto userDto = new UserDto();
    private readonly IUserService _userService;
    private readonly IOrderService _orderService;
    public MainViewModel(IAuthService authService, IUserService userService, IOrderService orderService)
    {
        _userService = userService;
        _orderService = orderService;
        GetUser();
    }
    [RelayCommand]
    private async Task GetUser()
    {
        if (IsBusy) return;

        IsBusy = true;
        IsRefreshing = true;
        try
        {
            UserDto = await _userService.GetUser();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error!", "Nie udało się pobrać użytkownika: " + ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }
    [RelayCommand]
    async Task GetAll()
    {
        if (IsBusy) return;

        try
        {
            IsRefreshing = true;
            IsBusy = true;
            var orders = await _orderService.GetAll(1, 10, UserDto.Email);

            if (Orders.Count != 0)
                Orders.Clear();

            foreach (var order in orders)
                Orders.Add(order);

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
}
