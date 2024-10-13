using PotoDocs.Services;

namespace PotoDocs.ViewModel;

[QueryProperty(nameof(PageTitle), "title")]
[QueryProperty(nameof(TransportOrderDto), "Transport order")]
public partial class TransportOrderFormViewModel : BaseViewModel
{
    [ObservableProperty]
    TransportOrderDto transportOrder;
    TransportOrderService transportOrderService;
    OpenAIService openAIService;

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
    public TransportOrderFormViewModel(TransportOrderService transportOrderService, OpenAIService openAIService)
    {
        this.transportOrderService = transportOrderService;
        this.openAIService = openAIService;
    }
}
