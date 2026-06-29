namespace SkyLogg.Client.Core.Components.Pages.BaseInfo;

public partial class BaseInfoPageBase : AppPageBase
{
    protected bool isFormOpen;
    protected bool isDeleteDialogOpen;
    protected bool isDiscardDialogOpen;
    protected string? deleteTargetName;
    private Action? pendingResetAction;

    protected void OpenForm() => isFormOpen = true;

    protected void CloseForm() => isFormOpen = false;

    protected void OpenAddForm(Action resetForm)
    {
        resetForm();
        OpenForm();
    }

    protected void OpenEditForm(Action loadForm)
    {
        loadForm();
        OpenForm();
    }

    protected void RequestCloseForm(bool hasUnsavedChanges, Action resetForm)
    {
        if (hasUnsavedChanges)
        {
            pendingResetAction = () =>
            {
                resetForm();
                CloseForm();
            };
            isDiscardDialogOpen = true;
            return;
        }

        resetForm();
        CloseForm();
    }

    protected void OpenDeleteDialog(string displayName)
    {
        deleteTargetName = displayName;
        isDeleteDialogOpen = true;
    }

    protected void CloseDeleteDialog()
    {
        isDeleteDialogOpen = false;
        deleteTargetName = null;
    }

    protected Task HandleDeleteCanceled()
    {
        CloseDeleteDialog();
        OnDeleteDialogCanceled();
        return Task.CompletedTask;
    }

    protected virtual void OnDeleteDialogCanceled()
    {
    }

    protected void RequestCancel(bool hasUnsavedChanges, Action resetAction)
    {
        RequestCloseForm(hasUnsavedChanges, resetAction);
    }

    protected void ConfirmDiscard()
    {
        isDiscardDialogOpen = false;
        pendingResetAction?.Invoke();
        pendingResetAction = null;
    }

    protected Task HandleDiscardConfirmed()
    {
        ConfirmDiscard();
        return Task.CompletedTask;
    }
}
