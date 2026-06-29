namespace SkyLogg.Client.Core.Components.Pages.BaseInfo;

public partial class BaseInfoPageBase : AppPageBase
{
    protected bool isDeleteDialogOpen;
    protected bool isDiscardDialogOpen;
    protected string? deleteTargetName;
    private Action? pendingResetAction;

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
        if (hasUnsavedChanges)
        {
            pendingResetAction = resetAction;
            isDiscardDialogOpen = true;
            return;
        }

        resetAction();
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
