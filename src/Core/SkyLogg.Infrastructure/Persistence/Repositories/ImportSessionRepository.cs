using SkyLogg.Application.Common.Interfaces;
using SkyLogg.Domain.Entities;

namespace SkyLogg.Infrastructure.Persistence.Repositories;

public sealed class ImportSessionRepository : IImportSessionRepository
{
    private static readonly List<ImportSession> Store = [];

    public Task<ImportSession?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        => Task.FromResult(Store.FirstOrDefault(s => s.Id == id && s.UserId == userId));

    public void Add(ImportSession session) => Store.Add(session);

    public void Update(ImportSession session)
    {
    }
}
