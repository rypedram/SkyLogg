# ✈️ Requirements Document - Pilot Logbook System

---

## 🎯 Purpose

This document defines the **functional and non-functional requirements** of the Aviation Pilot Logbook system.

It serves as the single source of truth for:

- Feature implementation
- Business rules
- System constraints
- Edge cases

---

# ✈️ 1. Flight Log Requirements

## 1.1 Core Flight Fields

Each flight must include:

- Flight Date
- Aircraft
- Aircraft Registration
- Departure Airport
- Arrival Airport
- Block Off Time
- Takeoff Time
- Landing Time
- Block On Time
- Flight Time (calculated)
- Block Time (calculated)
- PIC / SIC / Dual Time
- Night Time (optional)
- IFR Time (optional)
- Number of Landings
- Remarks
- Attachments (optional)

---

## 1.2 Time Calculation Rules

### Block Time

### Flight Time


Fallback rule:

If Takeoff/Landing missing:


---

## 1.3 Validation Rules

- Departure Airport ≠ Arrival Airport
- Block On > Block Off
- Takeoff ≥ Block Off
- Landing ≤ Block On
- Flight duration must be realistic (no negative or extreme values)
- Aircraft must exist and be active
- Airport must exist or be resolvable

---

## ✈️ 2. Aircraft Requirements

Each aircraft must include:

- Registration (unique per aircraft)
- Aircraft Type
- Manufacturer (optional enrichment)
- ICAO Code
- Wake Category
- Engine Count (optional)

---

## 🌍 3. Airport Requirements

Each airport must include:

- ICAO Code (mandatory)
- IATA Code (optional)
- Name
- Country
- City
- Latitude
- Longitude
- Timezone (if available)
- Elevation (optional)

---

## 📍 Airport Rules

- Airports must be uniquely identified by ICAO
- If missing, system must auto-resolve or create
- Coordinates are required for map visualization

---

## 🧠 4. AI / OCR Import Requirements

The system must support:

- Image input (camera/gallery)
- PDF input
- Multi-page documents
- Handwritten or printed logbooks (future support)

---

## 4.1 OCR Output Format

OCR must return **raw text only**.

AI layer will convert it to structured data.

---

## 4.2 AI Extraction Requirements

AI must extract:

- Flight rows
- Dates
- Times
- Airports
- Aircraft types
- Remarks

---

## 4.3 Confidence System

Each field must include:

- Value
- Confidence (0–100%)

Rules:

- ≥ 95% → auto-accept
- 80–94% → warning
- < 80% → user confirmation required

---

## 🌐 5. Master Data Requirements

System must support automatic resolution of:

- Airports
- Aircraft Types
- Countries
- Cities

If missing:

System must fetch from external aviation data providers.

---

## 📍 6. Flight Map Requirements

Map must support:

- Flight route visualization
- Airport markers
- Country visualization
- Heatmap
- Route clustering
- Animation playback (future-ready)

---

## Rules:

- Every airport must have coordinates
- Routes must be drawn as great-circle paths
- Map provider must be replaceable (abstraction layer)

---

## 📊 7. Statistics Requirements

System must calculate:

- Total flight hours
- Monthly/yearly hours
- Aircraft usage stats
- Airport frequency
- Country visits
- Longest flight
- Shortest flight
- Average flight time

---

## 🏆 8. Achievements Requirements

System must support:

- Flight milestones
- Landing milestones
- Night/IFR milestones
- Aircraft diversity achievements
- Airport diversity achievements

---

## 📦 9. Import/Export Requirements

### Import

- CSV
- Excel
- OCR Image
- PDF
- External logbook formats (future)

### Export

- PDF
- Excel
- CSV

---

## ⚙️ 10. System Constraints

- Mobile-first only (no web UI)
- Bit.BlazorUI only
- No external UI frameworks allowed
- Clean architecture mandatory
- Feature-based modular design required

---

## 🔒 11. Security Requirements

- JWT authentication
- Role-based access control (future-ready)
- Secure API communication
- Input validation on all layers
- Audit logging for critical operations

---

## 📡 12. Performance Requirements

- App must load dashboard in < 2 seconds
- OCR processing should be async
- Map rendering must support 1000+ flights smoothly
- Database queries must be optimized with indexes

---

## 🔮 13. Future Requirements (Do Not Implement Yet)

System must be designed for:

- Offline mode
- Sync engine
- Cloud backup
- Multi-device support
- AI learning system
- 3D globe visualization
- Wearable integration

---

## ❗ Important Rule

If any requirement is unclear:

👉 Do NOT assume  
👉 Ask before implementation