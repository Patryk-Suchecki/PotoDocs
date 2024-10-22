using PotoDocs.Services;

namespace PotoDocs.ViewModel;

[QueryProperty(nameof(PageTitle), "title")]
[QueryProperty(nameof(OrderDto), "OrderDto")]
[QueryProperty(nameof(InvoiceNumber), "InvoiceNumber")]
public partial class OrderFormViewModel : BaseViewModel
{
    [ObservableProperty]
    OrderDto orderDto;

    [ObservableProperty]
    bool isRefreshing;

    [ObservableProperty]
    int invoiceNumber;
    public ObservableCollection<UserDto> Users { get; } = new();

    OrderService orderService;
    UserService userService;
    IConnectivity connectivity;
    private UserDto selectedDriver;
    public UserDto SelectedDriver
    {
        get => selectedDriver;
        set
        {
            selectedDriver = value;
            if (OrderDto != null)
            {
                OrderDto.Driver = selectedDriver;
            }
            OnPropertyChanged();
        }
    }
    string pageTitle;
    public string PageTitle
    {
        get => pageTitle;
        set
        {
            pageTitle = value;
            Title = pageTitle;
        }
    }
    public OrderFormViewModel(OrderService orderService, UserService userService, IConnectivity connectivity)
    {
        this.orderService = orderService;
        this.userService = userService;
        this.connectivity = connectivity;

        GetDriversAsync();

    }
    [RelayCommand]
    async Task GetDriversAsync()
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
            var users = await userService.GetUsers();


            if (Users.Count != 0)
                Users.Clear();

            foreach (var user in users)
                Users.Add(user);
            SelectedDriver = OrderDto?.Driver != null ? Users.FirstOrDefault(u => u.Email == OrderDto.Driver.Email) : null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to get users: {ex.Message}");
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }

    }
    [RelayCommand]
    async Task SaveOrder()
    {
        if (orderDto == null)
            return;
        IsBusy = true;
        await orderService.UpdateOrderAsync(orderDto, invoiceNumber);
        IsBusy = false;
    }
}
