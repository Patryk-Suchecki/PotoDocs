using System.ComponentModel.DataAnnotations;
using PotoDocs.Services;
using PotoDocs.View;

namespace PotoDocs.ViewModel;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private LoginDto loginDto = new LoginDto();

    [ObservableProperty]
    private string errorText;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
        _authService.Logout();
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;

        var validationErrors = ValidateLoginDto(loginDto);
        if (validationErrors.Any())
        {
            ErrorText = string.Join("\n", validationErrors);
            IsBusy = false;
            return;
        }

        var error = await _authService.LoginAsync(loginDto);
        if (string.IsNullOrWhiteSpace(error))
        {
            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        }
        else
        {
            ErrorText = error;
        }

        IsBusy = false;
    }

    private List<string> ValidateLoginDto(LoginDto dto)
    {
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(dto, context, results, true);

        return results.Select(r => r.ErrorMessage).ToList();
    }
}
