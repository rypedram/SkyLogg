using Microsoft.Extensions.DependencyInjection;

namespace SkyLogg.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<Application.Common.Interfaces.IUnitOfWork, Persistence.UnitOfWork>();
        services.AddScoped<Application.Common.Interfaces.IFlightLogRepository, Persistence.Repositories.FlightLogRepository>();
        services.AddScoped<Application.Common.Interfaces.IAircraftRepository, Persistence.Repositories.AircraftRepository>();
        services.AddScoped<Application.Common.Interfaces.IAirportRepository, Persistence.Repositories.AirportRepository>();
        services.AddScoped<Application.Common.Interfaces.IImportSessionRepository, Persistence.Repositories.ImportSessionRepository>();

        services.AddScoped<Application.Features.Import.Services.IOcrProvider, External.Ocr.MockOcrProvider>();
        services.AddScoped<Application.Features.Import.Services.IAiExtractionProvider, External.Ai.RuleBasedAiExtractionProvider>();
        services.AddScoped<Application.Features.Import.Services.IImportOrchestrator, Features.Import.ImportOrchestrator>();
        services.AddScoped<Application.Features.Map.Services.IFlightMapDataService, Features.Map.FlightMapDataService>();

        return services;
    }
}
