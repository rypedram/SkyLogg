using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

[ApiVersion(1)]
[ApiController, Route("api/v{v:apiVersion}/[controller]/[action]")]
[Authorize(Policy = AuthPolicies.PRIVILEGED_ACCESS)]
[Authorize(Policy = AppFeatures.Logbook.ManageFlightLogs)]
public partial class CrewMemberController : AppControllerBase, ICrewMemberController
{
    [HttpGet, EnableQuery]
    public IQueryable<CrewMemberDto> Get()
    {
        var userId = User.GetUserId();
        return DbContext.CrewMembers.AsNoTracking().Where(c => c.UserId == userId).Project();
    }

    [HttpGet("{id}")]
    public async Task<CrewMemberDto> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        return await Get().FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.CrewMemberCouldNotBeFound)]);
    }

    [HttpPost]
    public async Task<CrewMemberDto> Create(CrewMemberDto dto, CancellationToken cancellationToken)
    {
        var entity = dto.Map();
        entity.UserId = User.GetUserId();
        entity.Id = Guid.NewGuid();

        await DbContext.CrewMembers.AddAsync(entity, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);

        return entity.Map();
    }

    [HttpPut]
    public async Task<CrewMemberDto> Update(CrewMemberDto dto, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var entity = await DbContext.CrewMembers
            .FirstOrDefaultAsync(c => c.Id == dto.Id && c.UserId == userId, cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.CrewMemberCouldNotBeFound)]);

        entity.Name = dto.Name;
        entity.DefaultRole = dto.DefaultRole;
        entity.Version = dto.Version;

        await DbContext.SaveChangesAsync(cancellationToken);
        return entity.Map();
    }

    [HttpDelete("{id}/{version}")]
    public async Task Delete(Guid id, long version, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var entity = await DbContext.CrewMembers
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.CrewMemberCouldNotBeFound)]);

        DbContext.CrewMembers.Remove(entity);
        entity.Version = version;

        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
