using PotoDocs.Services;
using PotoDocs.View;

namespace PotoDocs.ViewModel;

public partial class DriversViewModel : BaseViewModel
{
    public ObservableCollection<UserDto> Users { get; } = new();
    UserService userService;
    public DriversViewModel(UserService userService)
    {
        this.userService = userService;
        GetUsersAsync();
    }

    [ObservableProperty]
    bool isRefreshing;

    [RelayCommand]
    async Task GetUsersAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            var users = await userService.GetUsers();

            if (Users.Count != 0)
                Users.Clear();

            foreach (var user in users)
                Users.Add(user);

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to get users: {ex.Message}");
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }

    }
    [RelayCommand]
    async Task GoToNewUser()
    {
        await Shell.Current.GoToAsync(nameof(DriverFormPage), true, new Dictionary<string, object>
        {
            {"User", new UserDto() },
            {"title", "Dodaj kierowce" }
        });
    }
    [RelayCommand]
    async Task GoToEditUser(UserDto user)
    {
        if (user == null)
            return;

        await Shell.Current.GoToAsync(nameof(DriverFormPage), true, new Dictionary<string, object>
        {
            {"UserDto", user },
            {"title", "Edytuj kierowce" }
        });

    }
    [RelayCommand]
    async Task DeleteUser(UserDto transportOrder)
    {
        if (transportOrder == null)
            return;
    }
}