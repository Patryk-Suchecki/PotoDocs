using Microsoft.AspNetCore.Components;
using MudBlazor;
using PotoDocs.Shared.Models;

namespace PotoDocs.Blazor.Dialogs;

public partial class UserDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public UserDto UserDto { get; set; } = new UserDto();
    [Parameter] public List<string> Roles { get; set; } = [];

    private MudForm form = default!;
    private UserDtoValidator userValidator = new();

    private async Task SubmitAsync()
    {
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