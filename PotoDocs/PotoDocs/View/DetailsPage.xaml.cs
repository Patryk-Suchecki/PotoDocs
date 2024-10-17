namespace PotoDocs;

public partial class DetailsPage : ContentPage
{
    public DetailsPage(OrderDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}