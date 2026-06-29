namespace SkyLogg.Server.Api.Features.Logbook;

public partial class UserAchievementConfiguration : IEntityTypeConfiguration<UserAchievement>
{
    public void Configure(EntityTypeBuilder<UserAchievement> builder)
    {
        builder.HasKey(a => new { a.UserId, a.AchievementId });

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Achievement)
            .WithMany(a => a.UserAchievements)
            .HasForeignKey(a => a.AchievementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
