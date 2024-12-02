using PotoDocs.Services;
using PotoDocs.View;

namespace PotoDocs.ViewModel;

[QueryProperty(nameof(PageTitle), "title")]
[QueryProperty(nameof(UserDto), "UserDto")]
public partial class DriverFormViewModel : BaseViewModel
{
    [ObservableProperty]
    UserDto userDto;

    [ObservableProperty]
    bool isRefreshing;
    public ObservableDictionary<string, string> ValidationErrors { get; } = new();
    public ObservableCollection<string> Roles { get; } = new ();

    private readonly IAuthService _authService;

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

    public DriverFormViewModel(IAuthService authService)
    {
        _authService = authService;
        GetRoles();
    }

    [RelayCommand]
    public async Task GetRoles()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            var roles = await _authService.GetRoles();
            Roles.Clear();
            foreach (var role in roles)
            {
                Roles.Add(role);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error!", "Nie udało się pobrać ról. Spróbuj ponownie.", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    public async Task SaveDriver()
    {
        if (UserDto == null) return;

        if (!Validate()) return;
        try
        {
            await _authService.RegisterAsync(UserDto);
            await Shell.Current.GoToAsync($"//{nameof(DriversPage)}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", "Nie udało się zapisać kierowcy.", "OK");
        }
    }

    [RelayCommand]
    public Task GenerateNewPassword(UserDto driver)
    {
        if (driver == null)
        {
            Debug.WriteLine("Driver is null. Cannot generate a new password.");
            return Task.CompletedTask;
        }

        // Tu zaimplementuj logikę generowania hasła
        Debug.WriteLine($"New password for {driver.FirstAndLastName} has been generated.");
        return Task.CompletedTask;
    }
    private bool Validate()
    {
        ValidationErrors.Clear();

        var errors = ValidationHelper.ValidateToDictionary(UserDto);
        foreach (var error in errors)
        {
            ValidationErrors[error.Key] = error.Value;
        }

        OnPropertyChanged(nameof(ValidationErrors));
        return !errors.Any();
    }
}
