namespace PotoDocs;

public partial class DetailsPage : ContentPage
{
    public DetailsPage(TransportOrderDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}