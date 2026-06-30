using FluentValidation;
using MediatR;
using SkyLogg.Application.Common.Interfaces;
using SkyLogg.Application.Features.Aircraft.Dtos;
using SkyLogg.Application.Features.Aircraft.Validators;
using SkyLogg.Domain.Entities;
using SkyLogg.Domain.Services;

namespace SkyLogg.Application.Features.Aircraft.Commands;

public sealed record CreateAircraftCommand(AircraftWriteDto Aircraft) : IRequest<Guid>;

public sealed record UpdateAircraftCommand(Guid Id, long Version, AircraftWriteDto Aircraft) : IRequest;

public sealed record DeleteAircraftCommand(Guid Id, long Version) : IRequest;

public sealed class CreateAircraftCommandValidator : AbstractValidator<CreateAircraftCommand>
{
    public CreateAircraftCommandValidator() => RuleFor(x => x.Aircraft).SetValidator(new AircraftWriteDtoValidator());
}

public sealed class CreateAircraftCommandHandler : IRequestHandler<CreateAircraftCommand, Guid>
{
    private readonly IAircraftRepository repository;
    private readonly IUnitOfWork unitOfWork;

    public CreateAircraftCommandHandler(IAircraftRepository repository, IUnitOfWork unitOfWork)
    {
        this.repository = repository;
        this.unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateAircraftCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetAllAsync(cancellationToken);
        FlightValidationService.ValidateRegistrationUniqueness(request.Aircraft.Registration, existing);

        var entity = new Domain.Entities.Aircraft
        {
            Registration = request.Aircraft.Registration.Trim().ToUpperInvariant(),
            AircraftTypeId = request.Aircraft.AircraftTypeId,
            Type = request.Aircraft.Type,
            Model = request.Aircraft.Model
        };

        repository.Add(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
