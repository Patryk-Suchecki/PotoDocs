using PotoDocs.Services;
using PotoDocs.View;

namespace PotoDocs.ViewModel;

[QueryProperty(nameof(PageTitle), "title")]
[QueryProperty(nameof(UserDto), "UserDto")]
public partial class DriverFormViewModel : BaseViewModel
{
    private readonly IRoleService _roleService;
    private readonly IUserService _userService;

    [ObservableProperty]
    UserDto userDto = new UserDto();

    [ObservableProperty]
    bool isRefreshing;

    public ObservableDictionary<string, string> ValidationErrors { get; } = new();
    public ObservableCollection<string> Roles { get; } = new ();

    private string pageTitle;
    public string PageTitle
    {
        get => pageTitle;
        set
        {
            pageTitle = value;
            Title = pageTitle;
        }
    }
    private string selectedRole;
    public string SelectedRole
    {
        get => selectedRole;
        set
        {
            selectedRole = value;
            if (UserDto != null)
            {
                UserDto.Role = selectedRole;
            }
            OnPropertyChanged();
        }
    }

    public DriverFormViewModel(IRoleService roleService, IUserService userService)
    {
        _roleService = roleService;
        _userService = userService;
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

            var roles = await _roleService.GetRoles();
            Roles.Clear();
            foreach (var role in roles)
            {
                Roles.Add(role);
            }
            SelectedRole = UserDto?.Role != null ? Roles.FirstOrDefault(u => u == UserDto.Role) : null;
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
    public async Task Save()
    {
        if (UserDto == null) return;

        if (!Validate()) return;
        try
        {
            await _userService.RegisterAsync(UserDto);
            await Shell.Current.GoToAsync($"//{nameof(DriversPage)}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", "Nie udało się zapisać kierowcy.", "OK");
        }
    }

    [RelayCommand]
    public async Task GeneratePassword()
    {
        if (userDto == null) return;

        await _userService.GeneratePassword(userDto.Email);
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
