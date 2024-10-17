using PotoDocs.Services;

namespace PotoDocs.ViewModel;

[QueryProperty(nameof(PageTitle), "title")]
[QueryProperty(nameof(Order), "Order")]
public partial class OrderFormViewModel : BaseViewModel
{
    [ObservableProperty]
    OrderDto order;
    OrderService orderService;

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
    public OrderFormViewModel(OrderService orderService)
    {
        this.orderService = orderService;
    }
    [RelayCommand]
    async Task SaveOrder()
    {
        if (order == null)
            return;
        IsBusy = true;
        await orderService.CreateOrder(order); 
        IsBusy = false;
    }
}
