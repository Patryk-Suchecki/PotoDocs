using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace PotoDocs.Blazor.Dialogs;

public partial class MarkSentDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    private DateTime? _date = DateTime.Now;

    private void Submit()
    {
        if (_date.HasValue)
        {
            MudDialog.Close(DialogResult.Ok(_date.Value));
        }
        else
        {
            MudDialog.Close(DialogResult.Ok(DateTime.Now));
        }
    }

    private void Cancel() => MudDialog.Cancel();
}
