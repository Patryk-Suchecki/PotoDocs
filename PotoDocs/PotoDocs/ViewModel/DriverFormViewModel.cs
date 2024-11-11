using PotoDocs.Services;
using PotoDocs.View;

namespace PotoDocs.ViewModel;

[QueryProperty(nameof(PageTitle), "title")]
[QueryProperty(nameof(UserDto), "UserDto")]
public partial class DriverFormViewModel : BaseViewModel
{
    [ObservableProperty]
    UserDto userDto;
    private readonly IHttpClientFactory _httpClientFactory;
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
    public DriverFormViewModel(IHttpClientFactory httpClientFactory, IAuthService authService)
    {
        _authService = authService;
    }
    [RelayCommand]
    async Task SaveDriver(UserDto driver)
    {
        if (driver == null)
            return;
    }
    [RelayCommand]
    async Task GenerateNewPassword(UserDto driver)
    {
        if (driver == null)
            return;
    }
}

