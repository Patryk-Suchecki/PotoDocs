using BlazorDownloadFile;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using PotoDocs.Blazor.Services;
using PotoDocs.Shared.Models;

namespace PotoDocs.Blazor.Dialogs;

public partial class OrderDialog
{
    [Inject] private IOrderService OrderService { get; set; } = default!;
    [Inject] private IBlazorDownloadFileService BlazorDownloadFileService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public OrderDto OrderDto { get; set; } = new OrderDto();
    [Parameter] public List<UserDto> Users { get; set; } = [];
    [Parameter] public OrderFormType Type { get; set; } = OrderFormType.Update;

    private bool IsProcessingPdf = false;
    private bool IsProcessingCmr = false;
    private bool IsDisabled => Type == OrderFormType.Delete || Type == OrderFormType.Details;
    private IList<FileUploadDto> OrderFiles = [];
    private IList<FileUploadDto> CmrFiles = [];
    private IList<Guid> FileIdsToDelete = [];
    private long MaxFileSize = 512 * 1024 * 1024;

    private MudForm form = default!;
    private OrderDtoValidator orderValidator = new();
    private OrderStopDto stopBeforeEdit = new();

    private string ButtonLabel => Type switch
    {
        OrderFormType.Update => "Zapisz",
        OrderFormType.Delete => "Usuń",
        OrderFormType.Details => "Zamknij",
        _ => "Zapisz"
    };

    private static string TranslateType(StopType t) => t switch
    {
        StopType.Loading => "Załadunek",
        StopType.Unloading => "Rozładunek",
        _ => t.ToString()
    };

    protected override void OnParametersSet()
    {
        OrderDto ??= new OrderDto();
        OrderDto.Stops ??= [];
    }


    private async Task HandleFilesChanged(IReadOnlyList<IBrowserFile> files, FileType type)
    {
        try
        {
            foreach (var file in files)
            {
                if (!file.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    Snackbar.Add($"Plik '{file.Name}' nie jest PDF.", Severity.Error);
                    continue;
                }
                if (file.Size > MaxFileSize)
                {
                    Snackbar.Add($"Plik '{file.Name}' jest za duży.", Severity.Error);
                    continue;
                }

                await using var stream = file.OpenReadStream(MaxFileSize);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var fileBytes = ms.ToArray();

                var fileDto = new FileUploadDto
                {
                    Name = file.Name,
                    ContentType = file.ContentType,
                    Data = fileBytes
                };

                if (type == FileType.Cmr)
                {
                    CmrFiles.Add(fileDto);
                }
                else
                {
                    OrderFiles.Add(fileDto);
                }
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Błąd podczas dodawania pliku: {ex.Message}", Severity.Error);
        }
    }

    private async Task ParseOrder(FileUploadDto file)
    {
        try
        {
            IsProcessingPdf = true;
            OrderDto = await OrderService.ParseOrder(file);

            Snackbar.Add("Zlecenie uzupełnione automatycznie!", Severity.Success);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"{ex.Message}", Severity.Error);
        }
        finally
        {
            IsProcessingPdf = false;
        }
    }

    private void RemoveFile(FileUploadDto file)
    {
        if (!OrderFiles.Remove(file)) CmrFiles.Remove(file);
    }

    private async Task DownloadFile(FileUploadDto file)
    {
        await BlazorDownloadFileService.DownloadFile(file.Name, file.Data, file.ContentType);
    }

    private async Task ParseExistingOrder(OrderFileDto file)
    {
        try
        {
            IsProcessingPdf = true;
            OrderDto = await OrderService.ParseExistingOrder(file.Id);

            Snackbar.Add("Zlecenie uzupełnione automatycznie!", Severity.Success);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"{ex.Message}", Severity.Error);
        }
        finally
        {
            IsProcessingPdf = false;
        }
    }

    private void RemoveExistingFile(OrderFileDto file)
    {
        FileIdsToDelete.Add(file.Id);
        OrderDto.Files.Remove(file);
    }

    private async Task DownloadExistingFile(OrderFileDto dto)
    {
        try
        {
            if(dto.Type == FileType.Order)
            {
                IsProcessingPdf = true;
            }
            else
            {
                IsProcessingCmr = true;
            }

            var file = await OrderService.DownloadFile(dto.Id);
            await BlazorDownloadFileService.DownloadFile(file.FileName, file.FileContent, file.ContentType);
            Snackbar.Add($"Pobrano {file.FileName}", Severity.Info);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Błąd podczas pobierania faktury: {ex.Message}", Severity.Error);
        }
        finally
        {
            IsProcessingPdf = false;
            IsProcessingCmr = false;
        }
    }

    private async Task SubmitAsync()
    {
        if (Type == OrderFormType.Details)
        {
            MudDialog.Cancel();
            return;
        }

        if (Type == OrderFormType.Delete)
        {
            CloseDialogWithResult();
            return;
        }

        await form.Validate();
        if (form.IsValid)
        {
            CloseDialogWithResult();
        }
        else
        {
            Snackbar.Add("Formularz zawiera błędy. Popraw je i spróbuj ponownie.", Severity.Warning);
        }
    }

    private void CloseDialogWithResult()
    {
        var dialogResult = new OrderDialogResult
        {
            Order = OrderDto,
            OrderFiles = OrderFiles,
            CmrFiles = CmrFiles,
            FileIdsToDelete = FileIdsToDelete
        };

        MudDialog.Close(DialogResult.Ok(dialogResult));
    }
    private void RemoveStop(OrderStopDto stop)
    {
        OrderDto.Stops.Remove(stop);
    }
    private void AddStop()
    {
        OrderDto.Stops.Add(new OrderStopDto());
    }
    private void BackupStop(object item)
    {
        stopBeforeEdit = new()
        {
            Type = ((OrderStopDto)item).Type,
            Date = ((OrderStopDto)item).Date,
            Address = ((OrderStopDto)item).Address
        };
    }
    private void ResetStopToOriginalValues(object item)
    {
        ((OrderStopDto)item).Type = stopBeforeEdit.Type;
        ((OrderStopDto)item).Date = stopBeforeEdit.Date;
        ((OrderStopDto)item).Address = stopBeforeEdit.Address;
    }
}
public enum OrderFormType { Create, Details, Update, Delete }
public class OrderDialogResult
{
    public OrderDto Order { get; set; } = new OrderDto();
    public IEnumerable<FileUploadDto> OrderFiles { get; set; } = [];
    public IEnumerable<FileUploadDto> CmrFiles { get; set; } = [];
    public IEnumerable<Guid> FileIdsToDelete { get; set; } = [];
}
public class FileUploadDto
{
    public string Name { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] Data { get; set; } = [];
}