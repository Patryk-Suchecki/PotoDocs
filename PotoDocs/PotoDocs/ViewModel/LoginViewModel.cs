using PotoDocs.Services;
using PotoDocs.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PotoDocs.ViewModel;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    LoginDto loginDto;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
        _authService.Logout();
    }
    [RelayCommand]
    async Task GetTransportOrdersAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;

        var error = await _authService.LoginAsync(loginDto);
        if (string.IsNullOrWhiteSpace(error))
        {
            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        }
        else
        {
            ResponseLabel.Text = error;
        }
        IsBusy = false;
    }
}