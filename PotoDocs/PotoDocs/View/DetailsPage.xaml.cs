namespace PotoDocs.View;

public partial class DetailsPage : ContentPage
{
    public DetailsPage(OrderDetailsViewModel viewModel)
    {
        BindingContext = viewModel;

        InitializeComponent();
    }
}