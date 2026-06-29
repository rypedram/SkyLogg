using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook;

[ApiVersion(1)]
[ApiController, Route("api/v{v:apiVersion}/[controller]/[action]")]
[Authorize(Policy = AuthPolicies.PRIVILEGED_ACCESS)]
[Authorize(Policy = AppFeatures.BaseInfo.ManageCrew)]
public partial class CrewMemberController : AppControllerBase, ICrewMemberController
{
    [HttpGet, EnableQuery]
    public IQueryable<CrewMemberDto> Get()
    {
        var userId = User.GetUserId();
        return DbContext.CrewMembers.AsNoTracking().NotArchived().Where(c => c.UserId == userId).Project();
    }

    [HttpGet("{id}")]
    public async Task<CrewMemberDto> GetById(Guid id, CancellationToken cancellationToken)
    {
        return await Get().FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new ResourceNotFoundException(Localizer[nameof(AppStrings.CrewMemberCouldNotBeFound)]);
    }

    [HttpPost]
    public async Task<CrewMemberDto> Create(CrewMemberDto dto, CancellationToken cancellationToken)
    {
        PrepareCrewMemberDto(dto);

        var entity = dto.Map();
        entity.UserId = User.GetUserId();
        entity.Id = Guid.NewGuid();
        entity.IsArchived = false;

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

        PrepareCrewMemberDto(dto);

        entity.FirstName = dto.FirstName;
        entity.LastName = dto.LastName;
        entity.LicenceNumber = dto.LicenceNumber;
        entity.Birthday = dto.Birthday;
        entity.PositionType = dto.PositionType;
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

        entity.IsArchived = true;
        entity.Version = version;

        await DbContext.SaveChangesAsync(cancellationToken);
    }

    private static void PrepareCrewMemberDto(CrewMemberDto dto)
    {
        dto.FirstName = dto.FirstName?.Trim();
        dto.LastName = dto.LastName?.Trim();
        dto.LicenceNumber = string.IsNullOrWhiteSpace(dto.LicenceNumber) ? null : dto.LicenceNumber.Trim().ToUpperInvariant();
    }
}
