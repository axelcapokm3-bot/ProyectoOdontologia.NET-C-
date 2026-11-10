# AGENTS.md — Instrucciones del repositorio

Sistema odontológico en .NET 10 con Clean Architecture y Spec-Driven Development (SDD).

## Antes de actuar
1. Leer `docs/constitution.md` (autoridad y reglas).
2. Leer `docs/architecture.md` (stack y capas reales).
3. Respetar `docs/development-workflow.md` y `docs/security-policy.md`.
4. Consultar `memory/decisions.md` y `memory/lessons-learned.md`.
5. No modificar sin especificación aprobada.

## Stack
.NET 10, ASP.NET Core Web API, JWT Bearer, BCrypt, xUnit + Moq. **Sin ORM ni base de datos** (repositorios en memoria).

## Reglas no negociables
- Dominio puro: `CapaDominio` sin dependencias externas.
- Modelo rico: setters privados e invariantes en constructores.
- No introducir capas, patrones, ORM o dependencias sin spec aprobada.
- Nunca usar datos clínicos/personales reales en pruebas o ejemplos.
- Nunca versionar secretos; usar User Secrets o variables de entorno.
- No cambiar contratos públicos ni eliminar código sin aprobación.

## Flujo
IDEA → EXPLORACIÓN → ANÁLISIS → PREGUNTAS → DECISIÓN → SPEC → PLAN → TASKS → IMPLEMENTACIÓN → TESTS → REVIEW → VALIDACIÓN → MEMORY.

## Agentes
Coordinator (orquesta) · Explore (solo lectura) · Planner (spec/plan/tasks) · Implementer (código) · Reviewer (solo lectura).
Detalle en `docs/AGENTS.md`.

## Verificación
```powershell
dotnet build ProyectoOdontologia.slnx
dotnet test CapaPruebas\ProyectoOdontologia.Tests.csproj
powershell -ExecutionPolicy Bypass -File scripts/secret-scan.ps1
```
