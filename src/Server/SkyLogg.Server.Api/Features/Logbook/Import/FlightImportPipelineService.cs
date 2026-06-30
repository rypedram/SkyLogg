using System.Text.Json;
using SkyLogg.Shared.Features.Logbook;

namespace SkyLogg.Server.Api.Features.Logbook.Import;

public partial class FlightImportPipelineService : IFlightImportPipelineService
{
    [AutoInject] private AppDbContext dbContext = default!;
    [AutoInject] private IAirportResolutionService airportResolution = default!;
    [AutoInject] private IAircraftTypeResolutionService aircraftTypeResolution = default!;
    [AutoInject] private IFlightTimeCalculator flightTimeCalculator = default!;
    [AutoInject] private FlightLogService flightLogService = default!;
    [AutoInject] private IStringLocalizer<AppStrings> localizer = default!;
    [AutoInject] private IFlightOcrExtractionService ocrExtraction = default!;

    public async Task<FlightImportPreviewDto> PreviewFromFileAsync(
        Guid userId,
        Stream fileStream,
        string fileName,
        string? contentType,
        FlightImportSourceType sourceType,
        CancellationToken cancellationToken = default)
    {
        var rawText = await ocrExtraction.ExtractTextAsync(fileStream, fileName, contentType, cancellationToken);
        if (string.IsNullOrWhiteSpace(rawText))
            throw new BadRequestException(localizer[nameof(AppStrings.ImportOcrNoTextExtracted)]);

        return await PreviewAsync(userId, new FlightImportRequestDto
        {
            SourceType = sourceType,
            RawText = rawText,
            FileName = fileName,
        }, cancellationToken);
    }

    public async Task<FlightImportPreviewDto> PreviewAsync(Guid userId, FlightImportRequestDto request, CancellationToken cancellationToken = default)
    {
        var rows = FlightImportTextExtractor.ExtractRows(request.RawText!);
        if (rows.Count == 0)
            throw new BadRequestException(localizer[nameof(AppStrings.ImportNoFlightsRecognized)]);

        var resolvedAirports = new Dictionary<string, FlightImportResolvedAirportDto>(StringComparer.OrdinalIgnoreCase);
        var resolvedTypes = new Dictionary<string, FlightImportResolvedAircraftTypeDto>(StringComparer.OrdinalIgnoreCase);
        var candidates = new List<FlightImportCandidateDto>();

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        foreach (var row in rows)
        {
            var candidate = await BuildCandidateAsync(userId, row, resolvedAirports, resolvedTypes, cancellationToken);
            candidates.Add(candidate);
        }

        await ApplyDuplicateDetectionAsync(candidates, cancellationToken);

        var importHistory = new ImportHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SourceType = request.SourceType,
            FileName = request.FileName,
            Status = FlightImportStatus.AiParsed,
            RawText = request.RawText,
            ParsedJson = JsonSerializer.Serialize(candidates),
        };

        await dbContext.ImportHistories.AddAsync(importHistory, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new FlightImportPreviewDto
        {
            ImportHistoryId = importHistory.Id,
            Status = importHistory.Status,
            RawText = importHistory.RawText,
            Candidates = candidates,
            ResolvedAirports = resolvedAirports.Values.ToList(),
            ResolvedAircraftTypes = resolvedTypes.Values.ToList(),
            ValidCount = candidates.Count(c => c.IsValid),
            InvalidCount = candidates.Count(c => !c.IsValid),
            DuplicateCount = candidates.Count(c => c.IsDuplicate),
        };
    }

    public async Task<FlightImportConfirmResultDto> ConfirmAsync(Guid userId, FlightImportConfirmDto request, CancellationToken cancellationToken = default)
    {
        var importHistory = await dbContext.ImportHistories
            .FirstOrDefaultAsync(i => i.Id == request.ImportHistoryId && i.UserId == userId, cancellationToken)
            ?? throw new ResourceNotFoundException(localizer[nameof(AppStrings.ImportHistoryCouldNotBeFound)]);

        var flightsToSave = request.Flights;
        if (flightsToSave.Count == 0)
            throw new BadRequestException(localizer[nameof(AppStrings.NoFlights)]);

        var savedFlights = new List<FlightLogDto>();
        var skippedLineNumbers = new List<int>();

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        for (var i = 0; i < flightsToSave.Count; i++)
        {
            var flight = flightsToSave[i];
            var lineNumber = i < request.LineNumbers.Count ? request.LineNumbers[i] : i + 1;

            try
            {
                if (await flightLogService.HasAircraftOverlapAsync(flight, excludeFlightLogId: null, cancellationToken))
                {
                    skippedLineNumbers.Add(lineNumber);
                    continue;
                }

                await flightLogService.ValidateAndPrepareAsync(flight, userId, excludeFlightLogId: null, cancellationToken);

                var entity = new FlightLog { Id = Guid.NewGuid() };
                flightLogService.ApplyToEntity(entity, flight, userId);
                await dbContext.FlightLogs.AddAsync(entity, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);

                savedFlights.Add(LogbookMapper.MapFull(await dbContext.FlightLogs
                    .AsNoTracking()
                    .Include(f => f.Aircraft)
                    .Include(f => f.Sectors).ThenInclude(s => s.DepartureAirport)
                    .Include(f => f.Sectors).ThenInclude(s => s.ArrivalAirport)
                    .Include(f => f.CrewAssignments).ThenInclude(c => c.CrewMember)
                    .FirstAsync(f => f.Id == entity.Id, cancellationToken)));
            }
            catch (ConflictException)
            {
                skippedLineNumbers.Add(lineNumber);
            }
        }

        if (savedFlights.Count == 0 && skippedLineNumbers.Count > 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new FlightImportConfirmResultDto
            {
                SkippedDuplicateLineNumbers = skippedLineNumbers,
                SkippedDuplicateCount = skippedLineNumbers.Count,
            };
        }

        importHistory.Status = FlightImportStatus.Confirmed;
        importHistory.CompletedOn = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new FlightImportConfirmResultDto
        {
            SavedFlights = savedFlights,
            SavedCount = savedFlights.Count,
            SkippedDuplicateLineNumbers = skippedLineNumbers,
            SkippedDuplicateCount = skippedLineNumbers.Count,
        };
    }

    private async Task ApplyDuplicateDetectionAsync(List<FlightImportCandidateDto> candidates, CancellationToken cancellationToken)
    {
        var overlapMessage = localizer[nameof(AppStrings.FlightLogOverlapConflict)];
        var acceptedInBatch = new List<(Guid AircraftId, DateTimeOffset BlockOff, DateTimeOffset BlockOn)>();

        foreach (var candidate in candidates.OrderBy(c => c.LineNumber))
        {
            if (candidate.ProposedFlight is null || candidate.IsValid is false)
                continue;

            var flight = candidate.ProposedFlight;
            var sector = flight.Sectors.OrderBy(s => s.SectorOrder).FirstOrDefault();
            if (sector is null)
                continue;

            var isDuplicate = await flightLogService.HasAircraftOverlapAsync(flight, excludeFlightLogId: null, cancellationToken);

            if (isDuplicate is false)
            {
                isDuplicate = acceptedInBatch.Any(b =>
                    b.AircraftId == flight.AircraftId &&
                    sector.BlockOff < b.BlockOn &&
                    sector.BlockOn > b.BlockOff);
            }

            if (isDuplicate)
            {
                MarkAsDuplicate(candidate, overlapMessage);
                continue;
            }

            acceptedInBatch.Add((flight.AircraftId, sector.BlockOff, sector.BlockOn));
        }
    }

    private static void MarkAsDuplicate(FlightImportCandidateDto candidate, string overlapMessage)
    {
        candidate.IsDuplicate = true;
        candidate.IsValid = false;
        candidate.IsSelected = false;

        if (candidate.ValidationWarnings.Contains(overlapMessage) is false)
            candidate.ValidationWarnings.Add(overlapMessage);
    }

    private async Task<FlightImportCandidateDto> BuildCandidateAsync(
        Guid userId,
        FlightImportRow row,
        Dictionary<string, FlightImportResolvedAirportDto> resolvedAirports,
        Dictionary<string, FlightImportResolvedAircraftTypeDto> resolvedTypes,
        CancellationToken cancellationToken)
    {
        var warnings = new List<string>();
        var candidate = new FlightImportCandidateDto
        {
            LineNumber = row.LineNumber,
            FlightDate = row.FlightDate,
            AircraftType = row.Aircraft,
            DepartureAirportCode = row.DepartureIcao,
            ArrivalAirportCode = row.ArrivalIcao,
            BlockOff = row.BlockOff,
            Takeoff = row.Takeoff,
            Landing = row.Landing,
            BlockOn = row.BlockOn,
            Remarks = row.Remarks,
            IsSelected = true,
        };

        if (!row.FlightDate.HasValue)
            warnings.Add("Flight date is missing or invalid.");

        Airport? departure = null;
        Airport? arrival = null;

        if (string.IsNullOrWhiteSpace(row.DepartureIcao))
            warnings.Add("Departure airport is missing.");
        else
        {
            try
            {
                var (departureAirport, departureCreated) = await airportResolution.GetOrCreateAirportByCodeAsync(row.DepartureIcao, cancellationToken);
                departure = departureAirport;
                candidate.DepartureAirportId = departure.Id;
                TrackResolvedAirport(resolvedAirports, departure, departureCreated);
            }
            catch (KnownException ex)
            {
                warnings.Add($"Departure airport {row.DepartureIcao}: {ex.Message}");
            }
        }

        if (string.IsNullOrWhiteSpace(row.ArrivalIcao))
            warnings.Add("Arrival airport is missing.");
        else
        {
            try
            {
                var (arrivalAirport, arrivalCreated) = await airportResolution.GetOrCreateAirportByCodeAsync(row.ArrivalIcao, cancellationToken);
                arrival = arrivalAirport;
                candidate.ArrivalAirportId = arrival.Id;
                TrackResolvedAirport(resolvedAirports, arrival, arrivalCreated);
            }
            catch (KnownException ex)
            {
                warnings.Add($"Arrival airport {row.ArrivalIcao}: {ex.Message}");
            }
        }

        if (departure is not null && arrival is not null && departure.Id == arrival.Id)
            warnings.Add(localizer[nameof(AppStrings.DepartureArrivalMustDiffer)]);

        Aircraft? aircraft = null;
        if (string.IsNullOrWhiteSpace(row.Aircraft))
        {
            warnings.Add("Aircraft is missing.");
        }
        else if (IsRegistration(row.Aircraft))
        {
            candidate.AircraftRegistration = row.Aircraft.ToUpperInvariant();
            aircraft = await dbContext.Aircrafts
                .NotArchived()
                .FirstOrDefaultAsync(a => a.Registration == candidate.AircraftRegistration, cancellationToken);

            if (aircraft is null)
                warnings.Add($"Aircraft registration {candidate.AircraftRegistration} was not found.");
            else
                candidate.AircraftId = aircraft.Id;
        }
        else
        {
            var (aircraftType, typeCreated) = await aircraftTypeResolution.ResolveAircraftTypeAsync(row.Aircraft, cancellationToken);
            TrackResolvedAircraftType(resolvedTypes, aircraftType, typeCreated);

            aircraft = await GetOrCreateImportAircraftAsync(aircraftType, cancellationToken);
            candidate.AircraftId = aircraft.Id;
            candidate.AircraftRegistration = aircraft.Registration;
            candidate.AircraftType = $"{aircraftType.Manufacturer} {aircraftType.Model}";
        }

        if (!row.FlightDate.HasValue || aircraft is null || departure is null || arrival is null)
        {
            candidate.ValidationWarnings = warnings;
            candidate.IsValid = false;
            candidate.Confidence = 0.4;
            return candidate;
        }

        var blockOff = FlightImportTimeHelper.Combine(row.FlightDate.Value, row.BlockOff);
        var blockOn = FlightImportTimeHelper.Combine(row.FlightDate.Value, row.BlockOn);
        var takeoff = FlightImportTimeHelper.Combine(row.FlightDate.Value, row.Takeoff);
        var landing = FlightImportTimeHelper.Combine(row.FlightDate.Value, row.Landing);

        if (!blockOff.HasValue || !blockOn.HasValue)
            warnings.Add("Block off/on times are invalid.");

        if (blockOff.HasValue && blockOn.HasValue && blockOn <= blockOff)
            warnings.Add(localizer[nameof(AppStrings.BlockOnMustBeAfterBlockOff)]);

        if (takeoff.HasValue && blockOff.HasValue && takeoff < blockOff)
            warnings.Add(localizer[nameof(AppStrings.TakeoffMustBeAfterBlockOff)]);

        if (landing.HasValue && blockOn.HasValue && landing > blockOn)
            warnings.Add(localizer[nameof(AppStrings.LandingMustBeBeforeBlockOn)]);

        int blockMinutes = 0;
        int flightMinutes = 0;

        if (blockOff.HasValue && blockOn.HasValue)
        {
            (blockMinutes, flightMinutes) = flightTimeCalculator.Calculate(
                blockOff.Value, blockOn.Value, takeoff, landing);
            candidate.BlockTimeMinutes = blockMinutes;
            candidate.FlightTimeMinutes = flightMinutes;
        }

        var crewMember = await GetOrCreateDefaultCrewMemberAsync(userId, cancellationToken);

        if (blockOff.HasValue && blockOn.HasValue && warnings.Count == 0)
        {
            var proposed = new FlightLogDto
            {
                FlightDate = row.FlightDate.Value,
                AircraftId = aircraft.Id,
                AircraftRegistration = aircraft.Registration,
                AircraftType = candidate.AircraftType,
                Remarks = row.Remarks,
                Sectors =
                [
                    new FlightSectorDto
                    {
                        SectorOrder = 1,
                        DepartureAirportId = departure.Id,
                        DepartureAirportDisplay = $"{departure.ICAO} — {departure.Name}",
                        ArrivalAirportId = arrival.Id,
                        ArrivalAirportDisplay = $"{arrival.ICAO} — {arrival.Name}",
                        BlockOff = blockOff.Value,
                        BlockOn = blockOn.Value,
                        Takeoff = takeoff,
                        Landing = landing,
                        BlockTimeMinutes = blockMinutes,
                        FlightTimeMinutes = flightMinutes,
                        PicTimeMinutes = flightMinutes,
                        DayLandings = 1
                    }
                ],
                Crew =
                [
                    new FlightLogCrewDto
                    {
                        CrewMemberId = crewMember.Id,
                        CrewMemberName = crewMember.Name,
                        RoleType = CrewRoleType.PIC
                    }
                ],
                TotalBlockMinutes = blockMinutes,
                TotalFlightMinutes = flightMinutes,
                TotalPicMinutes = flightMinutes,
                TotalLandings = 1
            };

            candidate.ProposedFlight = proposed;
            candidate.IsValid = true;
            candidate.Confidence = 0.92;
        }
        else
        {
            candidate.IsValid = false;
            candidate.Confidence = Math.Max(0.3, 0.92 - warnings.Count * 0.1);
        }

        candidate.ValidationWarnings = warnings;
        return candidate;
    }

    private async Task<Aircraft> GetOrCreateImportAircraftAsync(AircraftType aircraftType, CancellationToken cancellationToken)
    {
        var placeholderRegistration = $"IMP-{aircraftType.TypeCode}";
        var existing = await dbContext.Aircrafts
            .NotArchived()
            .FirstOrDefaultAsync(a => a.Registration == placeholderRegistration, cancellationToken);

        if (existing is not null)
            return existing;

        var aircraft = new Aircraft
        {
            Id = Guid.NewGuid(),
            Registration = placeholderRegistration,
            AircraftTypeId = aircraftType.Id,
            Type = aircraftType.TypeCode,
            Model = aircraftType.Model,
            IsArchived = false
        };

        await dbContext.Aircrafts.AddAsync(aircraft, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return aircraft;
    }

    private async Task<CrewMember> GetOrCreateDefaultCrewMemberAsync(Guid userId, CancellationToken cancellationToken)
    {
        var existing = await dbContext.CrewMembers
            .NotArchived()
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.FirstName)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null)
            return existing;

        var crew = new CrewMember
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FirstName = "Self",
            LastName = "Pilot",
            DefaultRole = CrewRoleType.PIC,
            IsArchived = false
        };

        await dbContext.CrewMembers.AddAsync(crew, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return crew;
    }

    private static bool IsRegistration(string value)
    {
        var trimmed = value.Trim().ToUpperInvariant();
        if (trimmed.Contains(' '))
            return false;

        // Type designators such as MD-87, AN-24 — not tail numbers.
        if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^[A-Z]{2}-\d{1,3}$"))
            return false;

        return System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^[A-Z]{1,2}-[A-Z0-9]{2,5}$")
            || System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^N[0-9]{1,5}[A-Z]{0,2}$");
    }

    private static void TrackResolvedAirport(Dictionary<string, FlightImportResolvedAirportDto> map, Airport airport, bool wasCreated)
    {
        if (airport.ICAO is null || map.ContainsKey(airport.ICAO))
            return;

        map[airport.ICAO] = new FlightImportResolvedAirportDto
        {
            Id = airport.Id,
            Icao = airport.ICAO,
            Iata = airport.IATA,
            Name = airport.Name,
            Country = airport.Country,
            Latitude = airport.Latitude,
            Longitude = airport.Longitude,
            WasCreated = wasCreated
        };
    }

    private static void TrackResolvedAircraftType(Dictionary<string, FlightImportResolvedAircraftTypeDto> map, AircraftType type, bool wasCreated)
    {
        var key = type.TypeCode ?? type.Id.ToString();
        if (map.ContainsKey(key))
            return;

        map[key] = new FlightImportResolvedAircraftTypeDto
        {
            Id = type.Id,
            Manufacturer = type.Manufacturer,
            Model = type.Model,
            TypeCode = type.TypeCode,
            Category = type.Category,
            WasCreated = wasCreated
        };
    }
}
