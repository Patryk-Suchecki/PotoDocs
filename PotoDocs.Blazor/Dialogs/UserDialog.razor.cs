using Microsoft.AspNetCore.Components;
using MudBlazor;
using PotoDocs.Shared.Models;

namespace PotoDocs.Blazor.Dialogs;

public partial class UserDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public UserDto UserDto { get; set; } = new UserDto();
    [Parameter] public UserFormType Type { get; set; } = UserFormType.Update;
    [Parameter] public List<string> Roles { get; set; } = [];

    private bool IsDisabled => Type == UserFormType.Delete || Type == UserFormType.Details;
    private MudForm form = default!;
    private UserDtoValidator userValidator = new();

    private string ButtonLabel => Type switch
    {
        UserFormType.Update => "Zapisz",
        UserFormType.Delete => "Usuń",
        UserFormType.Details => "Zamknij",
        _ => "Zapisz"
    };

    private async Task SubmitAsync()
    {
        if (Type == UserFormType.Details)
        {
            MudDialog.Cancel();
            return;
        }

        if (Type == UserFormType.Delete)
        {
            CloseDialogWithResult();
            return;
        }

        await form.Validate();
        if (form.IsValid)
        {
            CloseDialogWithResult();
        }
    }

    private void CloseDialogWithResult()
    {
        MudDialog.Close(DialogResult.Ok(UserDto));
    }
}
public enum UserFormType { Create, Details, Update, Delete }