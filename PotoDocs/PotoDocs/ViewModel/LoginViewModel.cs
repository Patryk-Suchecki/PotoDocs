using PotoDocs.Services;
using PotoDocs.View;

namespace PotoDocs.ViewModel;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private LoginDto loginDto = new LoginDto();
    public ObservableDictionary<string, string> ValidationErrors { get; } = new();

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
        _authService.Logout();
    }

    [RelayCommand]
    public async Task Login()
    {
        if (IsBusy) return;
        if (!Validate()) return;

        IsBusy = true;

        var error = await _authService.LoginAsync(loginDto);
        if (string.IsNullOrWhiteSpace(error))
        {
            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        }
        else
        {
            ValidationErrors.Clear();
            ValidationErrors["General"] = error;
            OnPropertyChanged(nameof(ValidationErrors));
        }

        IsBusy = false;
    }
    private bool Validate()
    {
        ValidationErrors.Clear();

        var errors = ValidationHelper.ValidateToDictionary(LoginDto);
        foreach (var error in errors)
        {
            ValidationErrors[error.Key] = error.Value;
        }

        OnPropertyChanged(nameof(ValidationErrors));
        return !errors.Any();
    }
}
