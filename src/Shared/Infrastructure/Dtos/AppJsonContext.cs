using SkyLogg.Shared.Features.Dashboard;
using SkyLogg.Shared.Features.Statistics;
using SkyLogg.Shared.Features.Diagnostic;
using SkyLogg.Shared.Features.Logbook;
using CommunityToolkit.Datasync.Server.Abstractions.Json;

namespace SkyLogg.Shared.Infrastructure.Dtos;

/// <summary>
/// https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-source-generator/
/// </summary>
[JsonSourceGenerationOptions(

  Converters = [
    typeof(DateTimeConverter),
    typeof(DateTimeOffsetConverter),
    typeof(TimeOnlyConverter)
  ],

  AllowTrailingCommas = true,
  PropertyNameCaseInsensitive = true,
  GenerationMode = JsonSourceGenerationMode.Default,
  DictionaryKeyPolicy = JsonKnownNamingPolicy.CamelCase,
  PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase

)]


[JsonSerializable(typeof(Dictionary<string, JsonElement>))]
[JsonSerializable(typeof(Dictionary<string, string?>))]
[JsonSerializable(typeof(TimeSpan))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(Guid[]))]
[JsonSerializable(typeof(GitHubStats))]
[JsonSerializable(typeof(NugetStatsDto))]
[JsonSerializable(typeof(AppProblemDetails))]
[JsonSerializable(typeof(PushNotificationSubscriptionDto))]
[JsonSerializable(typeof(FlightLogDto))]
[JsonSerializable(typeof(PagedResponse<FlightLogDto>))]
[JsonSerializable(typeof(List<FlightLogDto>))]
[JsonSerializable(typeof(FlightSectorDto))]
[JsonSerializable(typeof(List<FlightSectorDto>))]
[JsonSerializable(typeof(FlightLogCrewDto))]
[JsonSerializable(typeof(List<FlightLogCrewDto>))]
[JsonSerializable(typeof(CrewMemberDto))]
[JsonSerializable(typeof(List<CrewMemberDto>))]
[JsonSerializable(typeof(AircraftDto))]
[JsonSerializable(typeof(List<AircraftDto>))]
[JsonSerializable(typeof(AircraftTypeDto))]
[JsonSerializable(typeof(List<AircraftTypeDto>))]
[JsonSerializable(typeof(AirportDto))]
[JsonSerializable(typeof(PagedResponse<AirportDto>))]
[JsonSerializable(typeof(List<AirportDto>))]
[JsonSerializable(typeof(CountryDto))]
[JsonSerializable(typeof(PagedResponse<CountryDto>))]
[JsonSerializable(typeof(List<CountryDto>))]
[JsonSerializable(typeof(CityDto))]
[JsonSerializable(typeof(PagedResponse<CityDto>))]
[JsonSerializable(typeof(List<CityDto>))]
[JsonSerializable(typeof(GeoTimeZoneDto))]
[JsonSerializable(typeof(PagedResponse<GeoTimeZoneDto>))]
[JsonSerializable(typeof(List<GeoTimeZoneDto>))]
[JsonSerializable(typeof(CrewPositionType))]
[JsonSerializable(typeof(FlightSummaryDto))]
[JsonSerializable(typeof(AircraftUsageStatDto))]
[JsonSerializable(typeof(List<AircraftUsageStatDto>))]
[JsonSerializable(typeof(AirportStatDto))]
[JsonSerializable(typeof(List<AirportStatDto>))]
[JsonSerializable(typeof(CurrencyStatusDto))]
[JsonSerializable(typeof(FlightMapDto))]
[JsonSerializable(typeof(FlightMapRouteDto))]
[JsonSerializable(typeof(List<FlightMapRouteDto>))]
[JsonSerializable(typeof(FlightMapPointDto))]
[JsonSerializable(typeof(List<FlightMapPointDto>))]
[JsonSerializable(typeof(FlightMapAirportPinDto))]
[JsonSerializable(typeof(List<FlightMapAirportPinDto>))]
[JsonSerializable(typeof(FlightMapCountryStatDto))]
[JsonSerializable(typeof(List<FlightMapCountryStatDto>))]
[JsonSerializable(typeof(FlightImportRequestDto))]
[JsonSerializable(typeof(FlightImportPreviewDto))]
[JsonSerializable(typeof(FlightImportCandidateDto))]
[JsonSerializable(typeof(List<FlightImportCandidateDto>))]
[JsonSerializable(typeof(FlightImportConfirmDto))]
[JsonSerializable(typeof(FlightImportConfirmResultDto))]
[JsonSerializable(typeof(FlightImportResolvedAirportDto))]
[JsonSerializable(typeof(List<FlightImportResolvedAirportDto>))]
[JsonSerializable(typeof(FlightImportResolvedAircraftTypeDto))]
[JsonSerializable(typeof(List<FlightImportResolvedAircraftTypeDto>))]
[JsonSerializable(typeof(FlightImportSourceType))]
[JsonSerializable(typeof(FlightImportStatus))]
[JsonSerializable(typeof(FlightStatisticsDto))]
[JsonSerializable(typeof(FlightStatisticItemDto))]
[JsonSerializable(typeof(MonthlyFlightTrendDto))]
[JsonSerializable(typeof(List<MonthlyFlightTrendDto>))]
[JsonSerializable(typeof(AchievementStatusDto))]
[JsonSerializable(typeof(List<AchievementStatusDto>))]
[JsonSerializable(typeof(OverallAnalyticsStatsDataResponseDto))]

public partial class AppJsonContext : JsonSerializerContext
{
}
