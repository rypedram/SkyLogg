namespace SkyLogg.Shared.Features.Logbook;

[Route("api/v1/[controller]/[action]/")]
[AuthorizedApi]
public interface ICrewMemberController : IAppController
{
    [HttpGet]
    Task<List<CrewMemberDto>> Get(CancellationToken cancellationToken) => default!;

    [HttpGet("{id}")]
    Task<CrewMemberDto> GetById(Guid id, CancellationToken cancellationToken);

    [HttpPost]
    Task<CrewMemberDto> Create(CrewMemberDto dto, CancellationToken cancellationToken);

    [HttpPut]
    Task<CrewMemberDto> Update(CrewMemberDto dto, CancellationToken cancellationToken);

    [HttpDelete("{id}/{version}")]
    Task Delete(Guid id, long version, CancellationToken cancellationToken);
}
