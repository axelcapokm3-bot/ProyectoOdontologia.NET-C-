---
name: dotnet-architecture-review
description: Use when reviewing or changing C#/.NET code in this odontología repo for Clean Architecture, DDD invariants, encapsulation and layer boundaries. Triggers on refactors of CapaDominio, service/repository changes, public contract changes, or "review arquitectura".
---

# .NET Architecture Review (Proyecto Odontología)

Aplicá esta lista al revisar cambios en este repositorio (.NET 10, Clean Architecture).

## Reglas de capas
- `CapaDominio` no referencia ninguna otra capa ni paquetes externos (salvo BCL).
- `CapaAplicacion` define interfaces; no referencia infraestructura.
- `CapaInfraestructura` implementa interfaces de aplicación; no filtra tipos externos al dominio.
- `CapaWebApi` no contiene lógica de negocio.
- Un cambio de contrato público requiere aprobación.

## Dominio rico
- Sin `public set` / `protected set`; usar `private set` o `init`.
- Guard clauses en constructores (`ArgumentException` / `ArgumentOutOfRangeException`).
- Comportamientos explícitos sobre estados: `Cancelar`, `Completar`, `Reprogramar`, `DescontarStock`, `ReponerStock`.
- Colecciones expuestas como `IReadOnlyCollection` o `AsReadOnly()`.

## Objetos de valor
- Inmutables y autovalidados.
- `IntervaloDeTiempo`, `AgendaDiaria` sin dependencias de infraestructura.
- Cálculo de huecos: si la fecha es hoy, el puntero no debe sugerir franjas pasadas.

## Calidad
- Nombres del dominio en español según convención existente.
- Código explícito, sin números mágicos, sin duplicación.
- Pruebas xUnit para cada comportamiento nuevo.
- Compilar (`dotnet build`) y probar (`dotnet test`) antes de aprobar.

## Salida esperada
Reporte por severidad (crítico/alto/medio/bajo) con `archivo:línea` y veredicto.
