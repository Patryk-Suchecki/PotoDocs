using PotoDocs.Services;
using PotoDocs.View;
using System.ComponentModel.DataAnnotations;

namespace PotoDocs.ViewModel;

[QueryProperty(nameof(PageTitle), "title")]
[QueryProperty(nameof(OrderDto), "OrderDto")]
[QueryProperty(nameof(InvoiceNumber), "InvoiceNumber")]
public partial class OrderFormViewModel : BaseViewModel
{
    [ObservableProperty]
    OrderDto orderDto;

    [ObservableProperty]
    bool isRefreshing;

    [ObservableProperty]
    int invoiceNumber;

    public ObservableCollection<UserDto> Users { get; } = new();
    public ObservableDictionary<string, string> ValidationErrors { get; } = new();

    private readonly IOrderService _orderService;
    private readonly IAuthService _authService;
    private readonly IConnectivity _connectivity;

    private UserDto selectedDriver;
    public UserDto SelectedDriver
    {
        get => selectedDriver;
        set
        {
            selectedDriver = value;
            if (OrderDto != null)
            {
                OrderDto.Driver = selectedDriver;
            }
            OnPropertyChanged();
        }
    }

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

    public OrderFormViewModel(IOrderService orderService, IAuthService authService, IConnectivity connectivity)
    {
        _orderService = orderService;
        _authService = authService;
        _connectivity = connectivity;

        GetAllDrivers();
    }

    [RelayCommand]
    async Task GetAllDrivers()
    {
        if (IsBusy) return;

        try
        {
            if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await Shell.Current.DisplayAlert("No connectivity!",
                    "Please check your internet and try again.", "OK");
                return;
            }

            IsBusy = true;
            var users = await _authService.GetAll();

            Users.Clear();
            foreach (var user in users)
            {
                Users.Add(user);
            }
            SelectedDriver = OrderDto?.Driver != null ? Users.FirstOrDefault(u => u.Email == OrderDto.Driver.Email) : null;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error!", "Nie udało się pobrać kierowców: " + ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    async Task Save()
    {
        if (OrderDto == null) return;
        if (!Validate()) return;

        IsBusy = true;
        try
        {
            await _orderService.Update(OrderDto, InvoiceNumber);
            await Shell.Current.GoToAsync($"//{nameof(OrdersPage)}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    async Task NavigateToPdf(string pdfname)
    {
        if (pdfname == null) return;

        IsBusy = true;
        try
        {
            string outputPath = await _orderService.DownloadFile(InvoiceNumber, pdfname);
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Zapisz pdf",
                File = new ShareFile(outputPath)
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    async Task RemoveCMR(string pdfname)
    {
        if (pdfname == null)
            return;

        IsBusy = true;
        try
        {
            await _orderService.RemoveCMR(OrderDto.InvoiceNumber, pdfname);
            OrderDto = await _orderService.GetById(InvoiceNumber);
            OnPropertyChanged(nameof(OrderDto));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error removing CMR: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    async Task AddCMRs()
    {
        try
        {
            var pdfFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, new[] { "com.adobe.pdf" } },
                { DevicePlatform.Android, new[] { "application/pdf" } },
                { DevicePlatform.WinUI, new[] { ".pdf" } },
                { DevicePlatform.MacCatalyst, new[] { "pdf" } }
            });

            var pickOptions = new PickOptions
            {
                PickerTitle = "Wybierz pliki PDF",
                FileTypes = pdfFileType
            };

            var results = await FilePicker.Default.PickMultipleAsync(pickOptions);
            if (results != null && results.Any())
            {
                IsBusy = true;

                var filePaths = results
                    .Where(result => result.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    .Select(result => result.FullPath)
                    .ToList();

                if (filePaths.Any())
                {
                    await _orderService.UploadCMR(filePaths, InvoiceNumber);
                    OrderDto = await _orderService.GetById(InvoiceNumber);
                    OnPropertyChanged(nameof(OrderDto));
                }
                else
                {
                    Debug.WriteLine("No valid PDF files selected.");
                }
            }
            else
            {
                Debug.WriteLine("No files selected.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
    private bool Validate()
    {
        ValidationErrors.Clear();

        var errors = ValidationHelper.ValidateToDictionary(OrderDto);
        foreach (var error in errors)
        {
            ValidationErrors[error.Key] = error.Value;
        }

        OnPropertyChanged(nameof(ValidationErrors));
        return !errors.Any();
    }
}
