using PotoDocs.Services;

namespace PotoDocs.ViewModel;

[QueryProperty(nameof(PageTitle), "title")]
[QueryProperty(nameof(TransportOrder), "Transport order")]
public partial class TransportOrderFormViewModel : BaseViewModel
{
    [ObservableProperty]
    TransportOrder transportOrder;
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
