# Decisiones

Registro breve de decisiones aprobadas. Una entrada por decisión.

| Fecha | Decisión | Justificación | Estado |
| --- | --- | --- | --- |
| 2026-11-09 | Adoptar entorno SDD con 5 agentes y gobernanza documental. | Estandarizar el flujo de desarrollo, trazabilidad y calidad. | Aprobada |
| 2026-11-09 | Dominio rico: setters privados e invariantes en constructores. | Migrar de modelo anémico a modelo rico. | Aprobada |
| 2026-11-09 | Sin ORM/base de datos por ahora. | El repositorio usa repositorios en memoria; no introducir tecnología sin spec. | Reemplazada |
| 2026-11-09 | Persistencia SQLite con EF Core (`Microsoft.EntityFrameworkCore.Sqlite`), dominio puro sin atributos EF. | El usuario aprobó persistencia real y transacciones para agenda. | Reemplazada |
| 2026-11-09 | Persistencia SQLite con Dapper + Microsoft.Data.Sqlite y SQL explícito; sin EF Core. `IDbConnectionFactory`, `UnitOfWorkSqlite` y repositorios SQLite para turnos/tratamientos. | El usuario priorizó control absoluto de consultas y aprendizaje de SQL; se descarta el ORM completo. | Aprobada |
| 2026-11-09 | `Turno` persiste `FechaHoraInicio`/`FechaHoraFin` escalares; `Horario` es calculado. | El índice `(OdontologoId, FechaHoraInicio)` queda operativo y las consultas usan SQL explícito. | Aprobada |
| 2026-11-09 | `Tratamiento.DuracionEstimada` es `TimeSpan`, persistido como ticks en `INTEGER`. | SQLite no tiene tipo nativo de intervalo; los ticks son ordenables y sin ambigüedad cultural. | Aprobada |
| 2026-11-09 | `POST /api/turnos` calcula el fin desde la duración del tratamiento; `PATCH /api/turnos/{id}/cancelar`; `GET /api/turnos/huecos`. | Contrato REST definido en `SYSTEM_HARNESS.md`. | Aprobada |
| 2026-11-10 | `UnitOfWorkSqlite.IniciarTransaccionAsync` ejecuta `BEGIN IMMEDIATE;` con `SqliteCommand`; no crea `IDbTransaction`. | Bloqueo pesimista de escritura a nivel de archivo para evitar carreras entre procesos; `TransaccionActiva` evita commits anticipados. | Aprobada |
| 2026-11-10 | El esquema inicial crea `Pacientes`, `Odontologos`, `Insumos`, `Tratamientos`, `TratamientoInsumos` y `Turnos`; las FK se limitan a tratamientos. | Cumplir el DDL completo sin romper los repositorios en memoria de pacientes, odontólogos e insumos. | Aprobada |
| 2026-11-10 | Dapper mapea directamente a `Turno` y `Tratamiento` con type handlers y type maps que fuerzan constructores ricos. | Evitar que Dapper use constructores privados/setters privados y omita invariantes al materializar. | Aprobada |
| 2026-11-10 | Excepciones de dominio (`DomainException`, `TurnoSolapadoException`, `PacienteNoEncontradoException`) y `ExceptionHandlingMiddleware` con `ProblemDetails`; controladores sin `try-catch`. | Centralizar el mapeo de errores a HTTP (400/404/409/500) y mantener los controladores como orquestadores. | Aprobada |
| 2026-11-10 | `TurnosService.RegistrarTurnoAsync` valida paciente y tratamiento dentro de `BEGIN IMMEDIATE`; se elimina el `SemaphoreSlim`. | La transacción de SQLite ya serializa la verificación de agenda y la persistencia entre procesos; el semáforo solo cubría una instancia. | Aprobada |
| 2026-11-10 | Repositorios SQLite de `Paciente` y `Odontologo` usan filas DTO internas (`PacienteRow`, `OdontologoRow`) y materializan la entidad con su constructor rico. | Dapper 2.1 falla al mapear constructores con `int?` y valores `Int64` de SQLite; la fila DTO evita `InvalidProgramException` sin abrir setters del dominio. | Aprobada |
| 2026-11-10 | `CapaPruebas` referencia `CapaWebApi` y `Microsoft.AspNetCore.App` para probar el middleware con `DefaultHttpContext`. | Permite verificar códigos HTTP y `ProblemDetails` sin levantar el servidor. | Aprobada |
| 2026-11-11 | Migrar `Insumo` a `InsumoRepositorySqlite`, unificar DDL con `MovimientosStock` y FK `TratamientoInsumos.InsumoId ON DELETE RESTRICT`; excepciones de dominio para odontólogo, tratamiento, insumo y stock insuficiente. | Persistir stock real, auditar cambios y mantener `Usuarios`/`RefreshTokens` fuera de alcance. Los servicios inician/confirman transacciones y dejan el rollback al `DisposeAsync` del UoW. | Aprobada |
