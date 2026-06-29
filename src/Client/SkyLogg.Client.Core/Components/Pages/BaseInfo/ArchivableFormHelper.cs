using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Client.Core.Components.Pages.BaseInfo;

internal static class ArchivableFormHelper
{
    public static bool GetIsActive(IArchivable entity) => !entity.IsArchived;

    public static void SetIsActive(IArchivable entity, bool isActive) => entity.IsArchived = !isActive;
}
