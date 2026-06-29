namespace SkyLogg.Server.Api.Features.Logbook;

public partial class ImportHistoryConfiguration : IEntityTypeConfiguration<ImportHistory>
{
    public void Configure(EntityTypeBuilder<ImportHistory> builder)
    {
        builder.HasIndex(i => new { i.UserId, i.CreatedOn });
        builder.Property(i => i.RawText).HasMaxLength(20000);
        builder.Property(i => i.ParsedJson).HasMaxLength(20000);

        builder.HasOne(i => i.User)
            .WithMany()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
