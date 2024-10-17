namespace PotoDocs.View;

public partial class OrderFormPage : ContentPage
{
	public OrderFormPage(OrderFormViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}