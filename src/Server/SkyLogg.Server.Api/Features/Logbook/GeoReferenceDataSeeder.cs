namespace SkyLogg.Server.Api.Features.Logbook;

public static class GeoReferenceDataSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Countries.AnyAsync(cancellationToken))
            return;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
