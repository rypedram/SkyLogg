# 🏗️ System Architecture - Pilot Logbook

---

## 🎯 Architecture Overview

This system is designed using a **modular, scalable, and Bit Platform-compliant architecture**.

It follows a **Clean Architecture + Feature-Based Modular Design** approach.

The system must be:

- Maintainable
- Testable
- Extensible
- AI-ready
- Offline-ready (future)

---

## 🧱 High-Level Architecture

The system consists of 4 main layers:



---

## 📱 1. Presentation Layer (Mobile)

Technology:

- .NET MAUI
- Bit.BlazorUI

Responsibilities:

- UI Rendering
- User Interaction
- Navigation
- State Management (Bit)
- Form Handling

Rules:

- NO business logic in UI
- NO calculations in UI
- NO direct DB access

---

## 🌐 2. API Layer

Technology:

- ASP.NET Core Web API

Responsibilities:

- Authentication
- Request validation
- Orchestration of services
- DTO mapping
- Error handling

Rules:

- No business logic
- No direct EF access
- Only delegates to Application layer

---

## 🧠 3. Application Layer (Core Logic)

This is the **brain of the system**.

Responsibilities:

- Flight creation logic
- Validation orchestration
- Import pipeline coordination
- AI processing coordination
- Map data preparation
- Statistics calculation triggers

Contains:

- Commands (CQRS style)
- Queries
- Use Cases
- DTOs

---

## 🧩 4. Domain Layer (Business Core)

This layer contains:

- Entities
- Value Objects
- Business Rules
- Domain Services

Entities:

- FlightLog
- Aircraft
- Airport
- Crew
- Country
- ImportSession

Rules:

- No external dependencies
- No EF Core references
- Pure business logic only

---

## 🗄️ 5. Infrastructure Layer

Responsible for:

- Database access (EF Core)
- External API integrations
- OCR providers
- AI providers
- Map services
- File storage

Examples:

- Airport Data Provider
- Aircraft Data Provider
- OCR Engine
- AI Extraction Engine

---

## 🧠 Feature-Based Structure

Instead of layering by technical type only, system must be feature-based:



Each feature contains:

- Commands
- Queries
- Services
- Validators
- Handlers
- UI components (if needed)

---

## ✈️ OCR & AI Pipeline Architecture

OCR system is **fully decoupled**.

Flow:



Rules:

- OCR does NOT understand aviation data
- AI does NOT directly write to DB
- Everything goes through validation layer

---

## 🗺️ Map Architecture

Map system must be provider-agnostic:

Interface:

- IMapProvider

Possible implementations:

- OpenStreetMap
- Google Maps (future)
- Custom tile provider

Responsibilities:

- Route rendering
- Airport markers
- Heatmap generation
- Animation playback

---

## 🧠 Master Data Resolution System

Before saving any Flight:

System checks:

- Airport existence
- Aircraft existence
- Country existence
- City existence

If missing:

- Fetch from external provider
- Auto-create entity
- Link automatically

---

## 🔄 Data Flow (Important)



---

## 🧾 Transaction Strategy

All Flight imports must be:

- Atomic
- Rollback-safe
- Consistent

If any step fails:

Entire import is rolled back.

---

## ⚡ Performance Rules

- Use async/await everywhere
- Use pagination for large datasets
- Use caching for:
  - Airports
  - Aircraft types
- Optimize map queries for bulk data

---

## 🧠 AI Integration Architecture

AI is treated as a **pluggable service**.

Interface:



Responsibilities:

- Convert OCR text → structured data
- Provide confidence scores
- Suggest corrections

AI providers can be replaced without code changes.

---

## 🔌 External Services Layer

System must support:

- Airport Data APIs
- Aircraft Data APIs
- Geolocation APIs
- AI Providers
- OCR Providers

All must be abstracted via interfaces.

---

## 🔐 Security Architecture

- JWT Authentication
- Role-based authorization (future)
- Secure API communication
- Input validation at every layer
- Audit logging for imports and deletes

---

## 📦 Deployment Architecture (Future)

System must support:

- Offline mobile mode (future)
- Cloud sync (future)
- Multi-device support (future)
- Background sync engine

---

## ❗ Critical Rules

- No business logic in UI
- No DB access outside Infrastructure
- No direct AI calls in UI or Domain
- Always use Application layer as orchestrator
- Always validate before persistence