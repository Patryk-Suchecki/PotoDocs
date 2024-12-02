using Microsoft.Maui.ApplicationModel;

namespace PotoDocs.ViewModel;

[QueryProperty(nameof(OrderDto), "OrderDto")]
public partial class OrderDetailsViewModel : BaseViewModel
{
    [ObservableProperty]
    OrderDto orderDto;
}