# 🗄️ Database Design - Pilot Logbook System (SQLite)

---

## 🎯 Purpose

This document defines the **SQLite database design** for the Pilot Logbook mobile application.

The system is optimized for:

- Mobile performance
- Offline-first capability
- Local storage efficiency
- Future sync readiness

---

## 🧱 Database Engine

- SQLite (Primary database on mobile)
- EF Core 10 SQLite Provider
- Optional future sync to cloud database

---

## ⚙️ Key Design Principles (SQLite Specific)

### 1. Mobile-first optimization
- Minimal joins
- Denormalized calculated fields where needed
- Fast read operations

---

### 2. Offline-first design
- All operations must work without internet
- Sync layer is future feature

---

### 3. Lightweight indexes
- Avoid heavy indexing
- Only critical search fields indexed

---

# 🧩 Core Tables

Structure remains same as Domain model but optimized for SQLite.

---

## 1. Users

```sql id="sqlite1"
Users
-----
Id TEXT PRIMARY KEY
FullName TEXT
Email TEXT UNIQUE
PasswordHash TEXT
CreatedAt TEXT
IsActive INTEGER

AircraftTypes
--------------
Id TEXT PRIMARY KEY
Manufacturer TEXT
Model TEXT
ICAOCode TEXT UNIQUE
IATACode TEXT
WakeCategory TEXT
EngineCount INTEGER
Family TEXT

Aircraft
--------
Id TEXT PRIMARY KEY
Registration TEXT UNIQUE
AircraftTypeId TEXT
IsActive INTEGER
CreatedAt TEXT

Airports
--------
Id TEXT PRIMARY KEY
ICAO TEXT UNIQUE
IATA TEXT
Name TEXT
City TEXT
Country TEXT
Latitude REAL
Longitude REAL
TimeZone TEXT
Elevation REAL


FlightLogs
----------
Id TEXT PRIMARY KEY
UserId TEXT
AircraftId TEXT

DepartureAirportId TEXT
ArrivalAirportId TEXT

FlightDate TEXT

BlockOff TEXT
Takeoff TEXT
Landing TEXT
BlockOn TEXT

BlockTimeMinutes INTEGER
FlightTimeMinutes INTEGER

PICMinutes INTEGER
SICMinutes INTEGER
DualMinutes INTEGER
InstructorMinutes INTEGER
SimulatorMinutes INTEGER

NightMinutes INTEGER
IFRMinutes INTEGER

LandingsCount INTEGER

Remarks TEXT

CreatedAt TEXT

FlightCrew
----------
Id TEXT PRIMARY KEY
FlightLogId TEXT
CrewId TEXT
RoleType TEXT

Crew
----
Id TEXT PRIMARY KEY
Name TEXT
Role TEXT

Attachments
-----------
Id TEXT PRIMARY KEY
FlightLogId TEXT
FileName TEXT
FileType TEXT
FilePath TEXT
UploadedAt TEXT

ImportSessions
--------------
Id TEXT PRIMARY KEY
UserId TEXT
SourceType TEXT
FileName TEXT
Status TEXT

TotalFlights INTEGER
SuccessfulFlights INTEGER
FailedFlights INTEGER
AverageConfidence REAL

CreatedAt TEXT
CompletedAt TEXT