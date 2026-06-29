namespace SkyLogg.Server.Api.Features.Logbook;

public partial class Achievement
{
    public Guid Id { get; set; }

    [Required, MaxLength(100)]
    public string? Code { get; set; }

    [Required, MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required, MaxLength(50)]
    public string? Metric { get; set; }

    public int Threshold { get; set; }

    public List<UserAchievement> UserAchievements { get; set; } = [];
}
