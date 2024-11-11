namespace PotoDocs.View;

public partial class DetailsPage : ContentPage
{
    public DetailsPage(OrderDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}