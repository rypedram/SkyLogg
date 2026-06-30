using FluentValidation;
using MediatR;
using SkyLogg.Application.Common.Interfaces;
using SkyLogg.Application.Features.Airports.Dtos;
using SkyLogg.Domain.Entities;
using SkyLogg.Domain.ValueObjects;

namespace SkyLogg.Application.Features.Airports.Commands;

public sealed record CreateAirportCommand(AirportWriteDto Airport) : IRequest<Guid>;

public sealed record ResolveAirportCommand(string Code) : IRequest<AirportReadDto?>;

public sealed class CreateAirportCommandValidator : AbstractValidator<CreateAirportCommand>
{
    public CreateAirportCommandValidator() => RuleFor(x => x.Airport).SetValidator(new Validators.AirportWriteDtoValidator());
}

public sealed class CreateAirportCommandHandler : IRequestHandler<CreateAirportCommand, Guid>
{
    private readonly IAirportRepository repository;
    private readonly IUnitOfWork unitOfWork;

    public CreateAirportCommandHandler(IAirportRepository repository, IUnitOfWork unitOfWork)
    {
        this.repository = repository;
        this.unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateAirportCommand request, CancellationToken cancellationToken)
    {
        _ = AirportCode.Create(request.Airport.Icao);

        var entity = new Airport
        {
            Icao = request.Airport.Icao.Trim().ToUpperInvariant(),
            Iata = request.Airport.Iata?.Trim().ToUpperInvariant(),
            Name = request.Airport.Name,
            CityId = request.Airport.CityId,
            CountryId = request.Airport.CountryId,
            Country = request.Airport.Country,
            Latitude = request.Airport.Latitude,
            Longitude = request.Airport.Longitude,
            ElevationFt = request.Airport.ElevationFt
        };

        repository.Add(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
