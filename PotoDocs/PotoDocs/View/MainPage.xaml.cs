using PotoDocs.Services;

namespace PotoDocs.View;

public partial class MainPage : ContentPage
{
    private readonly IAuthService _authService;
    public MainPage(IAuthService authService, MainPageViewModel viewModel)
    {
        InitializeComponent();
        _authService = authService;
        BindingContext = viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        //base.OnNavigatedTo(args);
        //if (await _authService.IsUserAuthenticated())
        //{
        //    await Shell.Current.GoToAsync($"//{nameof(OrdersPage)}");
        //}
        //else
        //{
        //    await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        //}
    }
}
