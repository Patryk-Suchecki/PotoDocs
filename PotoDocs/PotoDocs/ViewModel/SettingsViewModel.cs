using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PotoDocs.ViewModel;

public partial class SettingsViewModel : BaseViewModel
{
    // Automatyczna generacja właściwości i metody OnAreNotificationsEnabledChanged
    [ObservableProperty]
    private bool areNotificationsEnabled;

    public SettingsViewModel()
    {
        // Przykład - załaduj początkową wartość przełącznika
        //AreNotificationsEnabled = LoadNotificationPreference();
    }

    // Obsługa przycisku "Edytuj profil"
    [RelayCommand]
    private async Task EditProfileAsync()
    {
        await Shell.Current.DisplayAlert("Edytuj profil", "Otwieranie edycji profilu...", "OK");
        // Logika do otwierania widoku edycji profilu
    }

    // Obsługa przycisku "Zmień hasło"
    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        await Shell.Current.DisplayAlert("Zmień hasło", "Otwieranie zmiany hasła...", "OK");
        // Logika do otwierania widoku zmiany hasła
    }

    // Logika obsługująca powiadomienia
    private bool LoadNotificationPreference()
    {
        // Załaduj stan przełącznika z pamięci
        return Preferences.Get("AreNotificationsEnabled", true);
    }

    private void SaveNotificationPreference(bool isEnabled)
    {
        // Zapisz stan przełącznika do pamięci
        Preferences.Set("AreNotificationsEnabled", isEnabled);
    }

    //partial void OnAreNotificationsEnabledChanged(bool value)
    //{
    //    // Wywoływane przy każdej zmianie wartości przełącznika
    //    SaveNotificationPreference(value);
    //}
}
