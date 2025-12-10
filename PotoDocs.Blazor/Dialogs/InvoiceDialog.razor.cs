using Microsoft.AspNetCore.Components;
using MudBlazor;
using PotoDocs.Shared.Models;

namespace PotoDocs.Blazor.Dialogs;

public partial class InvoiceDialog
{
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public InvoiceDto InvoiceDto { get; set; } = new InvoiceDto();
    [Parameter] public InvoiceFormType Type { get; set; } = InvoiceFormType.Update;

    private bool IsDisabled => Type == InvoiceFormType.Delete || Type == InvoiceFormType.Details;

    private InvoiceItemDto elementBeforeEdit = new();

    private MudForm form = default!;
    private readonly InvoiceDtoValidator invoiceValidator = new();

    private string ButtonLabel => Type switch
    {
        InvoiceFormType.Update => "Zapisz",
        InvoiceFormType.Delete => "Usuń",
        InvoiceFormType.Details => "Zamknij",
        _ => "Zapisz"
    };

    protected override void OnParametersSet()
    {
        InvoiceDto ??= new InvoiceDto();
        InvoiceDto.Items ??= [];
    }
    private void AddNewItem()
    {
        InvoiceDto.Items.Add(new InvoiceItemDto());
    }

    private async Task SubmitAsync()
    {
        if (Type == InvoiceFormType.Details)
        {
            MudDialog.Cancel();
            return;
        }

        if (Type == InvoiceFormType.Delete)
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

        MudDialog.Close(DialogResult.Ok(InvoiceDto));
    }
    private void BackupItem(object item)
    {
        elementBeforeEdit = new()
        {
            Name = ((InvoiceItemDto)item).Name,
            Quantity = ((InvoiceItemDto)item).Quantity,
            Unit = ((InvoiceItemDto)item).Unit,
            NetPrice = ((InvoiceItemDto)item).NetPrice,
            VatRate = ((InvoiceItemDto)item).VatRate,
            GrossValue = ((InvoiceItemDto)item).GrossValue
        };
    }
    private void ResetItemToOriginalValues(object item)
    {
        ((InvoiceItemDto)item).Name = elementBeforeEdit.Name;
        ((InvoiceItemDto)item).Quantity = elementBeforeEdit.Quantity;
        ((InvoiceItemDto)item).Unit = elementBeforeEdit.Unit;
        ((InvoiceItemDto)item).NetPrice = elementBeforeEdit.NetPrice;
        ((InvoiceItemDto)item).VatRate = elementBeforeEdit.VatRate;
        ((InvoiceItemDto)item).GrossValue = elementBeforeEdit.GrossValue;
    }
    private static void OnItemCommit(object item)
    {
        ((InvoiceItemDto)item).NetValue = Math.Round(((InvoiceItemDto)item).Quantity * ((InvoiceItemDto)item).NetPrice, 2);
        ((InvoiceItemDto)item).VatAmount = Math.Round(((InvoiceItemDto)item).NetValue * ((InvoiceItemDto)item).VatRate, 2);
        ((InvoiceItemDto)item).GrossValue = ((InvoiceItemDto)item).NetValue + ((InvoiceItemDto)item).VatAmount;
    }
}
public enum InvoiceFormType { Create, Details, Update, Delete }