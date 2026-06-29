using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

public static class ArchivableQueries
{
    public const string NotArchivedIndexFilter = "[IsArchived] = 0";

    public static IQueryable<T> NotArchived<T>(this IQueryable<T> query) where T : class, IArchivable
        => query.Where(e => !e.IsArchived);
}
