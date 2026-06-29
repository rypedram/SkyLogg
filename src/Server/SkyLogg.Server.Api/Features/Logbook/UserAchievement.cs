using SkyLogg.Server.Api.Features.Identity.Models;

namespace SkyLogg.Server.Api.Features.Logbook;

public partial class UserAchievement
{
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    public Guid AchievementId { get; set; }

    [ForeignKey(nameof(AchievementId))]
    public Achievement? Achievement { get; set; }

    public DateTimeOffset UnlockedOn { get; set; } = DateTimeOffset.UtcNow;

    public Guid? SourceFlightId { get; set; }
}
