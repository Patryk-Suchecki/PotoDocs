using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PotoDocs.Services;
using PotoDocs.Shared.Models;
using PotoDocs.View;

namespace PotoDocs.ViewModel;

public partial class SettingsViewModel : BaseViewModel
{
    [ObservableProperty]
    bool isRefreshing;
    [ObservableProperty]
    private bool areNotificationsEnabled;
    [ObservableProperty]
    private UserDto userDto = new UserDto();
    [ObservableProperty]
    private ChangePasswordDto changePasswordDto = new ChangePasswordDto();
    private readonly IUserService _userService;
    public ObservableDictionary<string, string> ValidationErrors { get; } = new();

    public SettingsViewModel(IUserService userService)
    {
        _userService = userService;
        GetUser();
    }

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        if (IsBusy) return;
        if (!Validate()) return;
        IsBusy = true;
        IsRefreshing = true;
        try
        {
            ChangePasswordDto.Email = userDto.Email;
            await _userService.ChangePassword(ChangePasswordDto);
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }
        catch (Exception ex)
        {
            ValidationErrors.Clear();
            OnPropertyChanged(nameof(ValidationErrors));
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }
    [RelayCommand]
    private async Task GetUser()
    {
        if (IsBusy) return;

        IsBusy = true;
        IsRefreshing = true;
        try
        {
            UserDto = await _userService.GetUser();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error!", "Nie udało się pobrać użytkownika: " + ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    private bool LoadNotificationPreference()
    {
        return Preferences.Get("AreNotificationsEnabled", true);
    }

    private void SaveNotificationPreference(bool isEnabled)
    {
        Preferences.Set("AreNotificationsEnabled", isEnabled);
    }

    //partial void OnAreNotificationsEnabledChanged(bool value)
    //{
    //    // Wywoływane przy każdej zmianie wartości przełącznika
    //    SaveNotificationPreference(value);
    //}
    private bool Validate()
    {
        ValidationErrors.Clear();

        var errors = ValidationHelper.ValidateToDictionary(ChangePasswordDto);
        foreach (var error in errors)
        {
            ValidationErrors[error.Key] = error.Value;
        }

        OnPropertyChanged(nameof(ValidationErrors));
        return !errors.Any();
    }
}
