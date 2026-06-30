# 🧩 Domain Model - Pilot Logbook System

---

## 🎯 Purpose

This document defines the **core business domain model** of the Pilot Logbook system.

It represents the real-world aviation concepts translated into software entities.

No framework, no infrastructure, no UI concerns.

Pure business logic only.

---

# ✈️ 1. Core Entity: FlightLog

## FlightLog

Represents a single flight sector.

### Properties

- Id (GUID)
- UserId
- FlightDate
- AircraftId
- DepartureAirportId
- ArrivalAirportId

### Time Fields

- BlockOff
- Takeoff
- Landing
- BlockOn

### Calculated Fields (NOT stored, computed)

- BlockTime
- FlightTime
- NightTime
- IFRTime

### Operational Fields

- PICTime
- SICTime
- DualTime
- InstructorTime
- SimulatorTime

### Additional Fields

- NumberOfLandings
- Remarks
- HasNightFlight (bool)
- HasIFR (bool)

---

## Business Rules

- DepartureAirportId != ArrivalAirportId
- BlockOn > BlockOff
- Takeoff >= BlockOff
- Landing <= BlockOn

### Time Calculation Rule

Fallback:

If Takeoff or Landing missing:


---

# 🛩️ 2. Aircraft Entity

Represents a specific aircraft registration.

### Properties

- Id
- Registration (UNIQUE)
- AircraftTypeId
- IsActive

---

## AircraftType

Represents aircraft model information.

### Properties

- Id
- Manufacturer
- Model
- ICAOCode
- IATACode
- WakeCategory
- EngineCount
- Family

---

## Business Rules

- Registration must be unique globally
- AircraftType must exist before assigning Aircraft
- Inactive aircraft cannot be used in FlightLog

---

# 🌍 3. Airport Entity

Represents an airport in the system.

### Properties

- Id
- ICAO (UNIQUE)
- IATA (optional)
- Name
- City
- Country
- Latitude
- Longitude
- TimeZone
- Elevation

---

## Business Rules

- ICAO is primary unique identifier
- Latitude & Longitude are required for map rendering
- Airports must be resolvable via external provider if missing

---

# 👨‍✈️ 4. Crew Entity

Represents crew members assigned to a flight.

### Properties

- Id
- Name
- Role (PIC / SIC / Cabin / Instructor)

---

# ✈️ 5. FlightCrew (Junction Entity)

Links Crew to FlightLog.

### Properties

- FlightLogId
- CrewId
- RoleType

---

# 🌎 6. Country Entity

### Properties

- Id
- Name
- ISOCode

---

# 🏙️ 7. City Entity

### Properties

- Id
- Name
- CountryId

---

# 📎 8. Attachment Entity

Used for flight documentation.

### Properties

- Id
- FlightLogId
- FileName
- FileType
- FilePath
- UploadedAt

---

# 📥 9. ImportSession Entity

Tracks OCR / AI imports.

### Properties

- Id
- UserId
- SourceType (Image / PDF / CSV)
- FileName
- Status
- CreatedAt
- CompletedAt

---

## Import Status

- Pending
- Processing
- Completed
- Failed

---

# 🧠 10. Value Objects

---

## TimeRange

Represents aviation time intervals.

- StartTime
- EndTime

---

## FlightTime

Represents computed flight duration.

---

## AirportCode

Encapsulates ICAO/IATA validation.

---

# 🔗 Entity Relationships



---

# 🧠 Domain Rules (Critical)

## Flight Integrity Rules

- No flight can exist without Aircraft
- No flight can exist without Departure and Arrival
- Time values must be logically consistent

---

## Aircraft Rules

- Aircraft must be active to be used
- Registration is immutable once created

---

## Airport Rules

- ICAO is immutable
- Coordinates must always exist

---

## Crew Rules

- Crew can exist independently
- Crew can be reused across flights

---

# 🧠 Domain Services

## FlightCalculationService

Responsible for:

- FlightTime calculation
- BlockTime calculation
- Night/IFR estimation

---

## FlightValidationService

Responsible for:

- Business rule validation
- Time consistency checks
- Route validation

---

## AirportResolutionService

Responsible for:

- Resolving ICAO codes
- Fetching missing airport data

---

# ⚠️ Domain Constraints

- Domain must NOT depend on:
  - Database
  - UI
  - External APIs
  - OCR
  - AI

Only pure business logic is allowed.

---

# 🔮 Future Extensions

Domain is designed to support:

- Offline flight logging
- Multi-user sync
- AI-assisted corrections
- Flight pattern learning
- Global aviation analytics