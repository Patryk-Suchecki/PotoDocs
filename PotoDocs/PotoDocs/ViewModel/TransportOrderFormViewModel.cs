using PotoDocs.Services;

namespace PotoDocs.ViewModel;

[QueryProperty(nameof(PageTitle), "title")]
[QueryProperty(nameof(TransportOrder), "Transport order")]
public partial class TransportOrderFormViewModel : BaseViewModel
{
    [ObservableProperty]
    TransportOrderDto transportOrder;
    TransportOrderService transportOrderService;

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
    public TransportOrderFormViewModel(TransportOrderService transportOrderService)
    {
        this.transportOrderService = transportOrderService;
    }
}
