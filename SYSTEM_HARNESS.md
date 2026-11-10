# SYSTEM HARNESS — Proyecto Odontología

Este documento es la **constitución de ejecución** del proyecto. Tiene prioridad sobre
cualquier otra guía (AGENTS.md, docs/, inferencias). Si hay conflicto, manda este archivo.

## 1. Autoridad

1. Instrucciones explícitas del usuario.
2. Este SYSTEM_HARNESS.md.
3. `docs/constitution.md`.
4. `AGENTS.md` y `docs/`.
5. Especificaciones aprobadas (`spec/`).
6. Inferencias del agente (nunca suficientes para cambiar arquitectura, contratos o datos).

Ningún agente puede interpretar una propuesta como aprobación.

## 2. Arquitectura obligatoria

- Clean Architecture / Arquitectura Cebolla en .NET 10.
- `CapaDominio`: puro. Sin EF, sin atributos de persistencia, sin dependencias externas.
- `CapaAplicacion`: casos de uso, DTOs, interfaces (puertos). Sin EF.
- `CapaInfraestructura`: Dapper + SQLite, repositorios con SQL explícito, Unit of Work.
- `CapaWebApi`: endpoints, DI, manejo semántico de errores. Sin lógica de negocio.
- Prohibido exponer setters públicos en el dominio. La materialización usa constructores ricos.

## 3. Modelo de dominio rico

- Invariantes en constructores (`ArgumentException` / `ArgumentOutOfRangeException`).
- Comportamientos explícitos sobre estado (`Cancelar`, `Completar`, `Reprogramar`, `DescontarStock`, `ReponerStock`).
- `AgendaDiaria` es el Aggregate Root de agenda: encapsula turnos por odontólogo/día,
  detecta solapamientos y calcula huecos.
- `Tratamiento.DuracionEstimada` es `TimeSpan` (no `int`).

## 4. Persistencia

- Motor: SQLite (`Microsoft.Data.Sqlite`) con Dapper (micro-ORM aprobado).
- SQL explícito y parametrizado en los repositorios; sin ORM completo ni atributos en el dominio.
- `IDbConnectionFactory` crea una conexión nueva; `UnitOfWorkSqlite` comparte esa conexión con los repositorios.
- `UnitOfWorkSqlite.IniciarTransaccionAsync` ejecuta `BEGIN IMMEDIATE;` mediante `SqliteCommand` para bloqueo pesimista a nivel de archivo; `COMMIT;`/`ROLLBACK;` también se ejecutan como SQL explícito.
- Índice compuesto en `Turno`: `OdontologoId` + fecha/hora de inicio.
- Escrituras de agenda dentro de transacción (`IUnitOfWork` / `UnitOfWorkSqlite`).
- `DatabaseInitializer` ejecuta `CREATE TABLE IF NOT EXISTS ...` para `Pacientes`, `Odontologos`, `Insumos`, `Tratamientos`, `TratamientoInsumos` y `Turnos`, más sus índices.
- Dapper mapea directamente a las entidades con type handlers y constructores ricos forzados; no se usan setters públicos ni constructores sin parámetros para materializar.

## 5. Contratos REST de turnos

- `GET /api/turnos/huecos?odontologoId=&fecha=&duracionMinutos=`
- `POST /api/turnos` — el cliente **no** envía `FechaHoraFin`; se calcula desde `Tratamiento.DuracionEstimada`.
- `PATCH /api/turnos/{id}/cancelar`

Mapeo de errores:
- `InvalidOperationException` → 409 Conflict.
- `ArgumentException` → 400 Bad Request.
- `KeyNotFoundException` → 404 Not Found.

## 6. Asincronía

`CancellationToken` se propaga por toda la cadena (controller → servicio → repositorio → Dapper).

## 7. Calidad y verificación

- Nada de lógica de negocio en controllers.
- Nada de algoritmos de agenda en services: el service orquesta y delega en el dominio.
- Pruebas xUnit actualizadas al diseño rico; la suite debe quedar en verde.
- Verificación obligatoria: `dotnet build`, `dotnet test`, `scripts/secret-scan.ps1`.

## 8. Seguridad y datos

- Prohibido usar datos clínicos/personales reales en pruebas o ejemplos.
- Sin secretos versionados; usar User Secrets o variables de entorno.
- No registrar datos sensibles en logs ni respuestas de error.

## 9. Prohibiciones

- No agregar capas, ORM completos, patrones o dependencias sin aprobación.
- No modificar contratos públicos ni eliminar código sin aprobación.
- No introducir lógica de negocio en infraestructura ni en la API.
