using Microsoft.AspNetCore.Components;
using MudBlazor;
using PotoDocs.Shared.Models;

namespace PotoDocs.Blazor.Dialogs;

public partial class InvoiceCorrectionDialog
{
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public InvoiceCorrectionDto InvoiceCorrectionDto { get; set; } = default!;
    [Parameter] public InvoiceCorrectionFormType Type { get; set; } = InvoiceCorrectionFormType.Update;

    private bool IsDisabled => Type == InvoiceCorrectionFormType.Delete || Type == InvoiceCorrectionFormType.Details;

    private InvoiceItemDto elementBeforeEdit = new();

    private MudForm form = default!;
    private readonly InvoiceCorrectionDtoValidator invoiceCorrectionValidator = new();

    private string ButtonLabel => Type switch
    {
        InvoiceCorrectionFormType.Update => "Zapisz",
        InvoiceCorrectionFormType.Delete => "Usuń",
        InvoiceCorrectionFormType.Details => "Zamknij",
        _ => "Zapisz"
    };
    private void AddNewItem()
    {
        InvoiceCorrectionDto.Items.Add(new InvoiceItemDto());
    }

    private async Task SubmitAsync()
    {
        if (Type == InvoiceCorrectionFormType.Details)
        {
            MudDialog.Cancel();
            return;
        }

        if (Type == InvoiceCorrectionFormType.Delete)
        {
            CloseDialogWithResult();
            return;
        }

        await form.Validate();
        if (form.IsValid && InvoiceCorrectionDto.Items.Count != 0)
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

        MudDialog.Close(DialogResult.Ok(InvoiceCorrectionDto));
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
    private void CopyItemToCorrection(InvoiceItemDto dto)
    {
        var item = new InvoiceItemDto
        {
            Id = Guid.Empty,
            Name = dto.Name,
            Unit = dto.Unit,
            Quantity = dto.Quantity,
            NetPrice = dto.NetPrice,
            VatRate = dto.VatRate,
            NetValue = dto.NetValue,
            VatAmount = dto.VatAmount,
            GrossValue = dto.GrossValue
        };

        InvoiceCorrectionDto.Items.Add(item);
    }
    private void RemoveItem(InvoiceItemDto item)
    {
        InvoiceCorrectionDto.Items.Remove(item);
    }
}
public enum InvoiceCorrectionFormType { Create, Details, Update, Delete }