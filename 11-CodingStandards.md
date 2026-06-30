# 🧑‍💻 Coding Standards - Pilot Logbook System

---

## 🎯 Purpose

This document defines the **strict coding standards** for the entire project.

All developers and AI agents (Cursor) MUST follow these rules.

---

# 🧱 1. General Principles

- Clean Code first
- Simplicity over complexity
- Readability over cleverness
- Maintainability over shortcuts

---

# ⚙️ 2. Architecture Rules

## Mandatory:

- Must follow Clean Architecture
- Must follow Bit Platform structure
- Feature-based organization is REQUIRED

---

## Forbidden:

- No business logic in UI
- No direct DB access outside Infrastructure
- No logic inside controllers
- No mixing features in one module

---

# 🧩 3. Project Structure Rules


Each feature MUST contain:

- Commands
- Queries
- Services
- Validators
- DTOs

---

# 🧠 4. Naming Conventions

## Classes:

PascalCase


---

## Methods:

PascalCase


---

## Variables:

camelCase


---

## Database Fields:

PascalCase (EF Core mapping allowed)

---

# ⚡ 5. Async Rules

- ALL I/O operations must be async
- NEVER use `.Result` or `.Wait()`
- Always use `async/await`

---

# 🧱 6. Dependency Injection Rules

- ALL services must be injected
- NEVER instantiate services manually
- Interfaces required for all external dependencies

---

# 🧠 7. Business Logic Rules

- Business logic belongs ONLY in Domain or Application layer
- UI must NEVER contain logic
- API controllers must be thin

---

# 📦 8. Error Handling Rules

- Never swallow exceptions
- Always log critical errors
- Use structured error responses
- No generic “try-catch and ignore”

---

# 🧾 9. Validation Rules

- Validate in Application layer
- Domain rules must be enforced
- UI validation is only for UX

---

# 🧠 10. AI & OCR Rules

- AI never writes to database
- OCR returns raw text only
- AI output must always be validated
- Confidence scoring is mandatory

---

# 🗺️ 11. Map Rules

- Map is visualization ONLY
- No business logic inside map components
- All data must come from Application layer

---

# 🧱 12. Code Complexity Rules

- Methods max 30–40 lines preferred
- If longer → split into services
- Avoid nested conditions > 3 levels

---

# 🔁 13. Reusability Rules

- No duplicated logic
- Shared logic must go into Services or Helpers
- No copy-paste between features

---

# ⚡ 14. Performance Rules

- Avoid N+1 queries
- Use pagination for lists
- Cache static data (Airports, AircraftTypes)
- Optimize DB queries

---

# 🧪 15. Testing Rules

- Critical business logic must be testable
- Domain layer must be unit-test friendly
- No external dependencies in tests

---

# 🚫 16. Forbidden Practices

- No magic strings
- No hardcoded IDs
- No business logic in controllers
- No UI logic in services
- No direct SQL queries in application layer

---

# 🧭 17. Cursor AI Rules

When using Cursor:

- Always read `/docs` first
- Always propose plan before coding
- Never implement multiple features at once
- Never assume missing requirements
- Ask questions when uncertain

---

# 🔮 18. Future-Proof Rules

Code must be:

- Extensible
- Replaceable (AI providers, OCR, Map)
- Modular
- Decoupled