# Arquitectura del Sistema Odontológico

> Estado verificado el 2026-11-09. Fuente: inspección de `.csproj`, estructura de carpetas y configuración.

## Stack verificado

| Aspecto | Valor |
| --- | --- |
| Runtime / SDK | .NET 10 (`net10.0`), SDK 10.0.201 |
| Lenguaje | C# (Nullable + ImplicitUsings habilitados) |
| Tipo de app | ASP.NET Core Web API (`Microsoft.NET.Sdk.Web`) |
| Autenticación | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) |
| Hash de contraseñas | BCrypt.Net-Next |
| Documentación API | Swashbuckle / OpenAPI |
| Pruebas | xUnit + Moq + coverlet |
| Acceso a datos | Dapper 2.1 (micro-ORM) con SQL explícito |
| Base de datos | SQLite (`Microsoft.Data.Sqlite`) |

## Capas (Clean Architecture)

```
CapaWebApi (API)          → expone endpoints, configura DI, JWT, middleware
    ↓
CapaAplicacion (Application) → servicios de aplicación, DTOs, interfaces, seguridad
    ↓
CapaDominio (Domain)      → entidades, objetos de valor, invariantes (sin dependencias)
    ↑
CapaInfraestructura       → implementaciones de repositorios y servicios (BCrypt, JWT)
CapaPruebas (Tests)       → xUnit
```

Referencias reales:
- `CapaAplicacion` → `CapaDominio`
- `CapaInfraestructura` → `CapaDominio`, `CapaAplicacion`
- `CapaWebApi` → `Dominio`, `Aplicacion`, `Infraestructura`
- `CapaPruebas` → `CapaInfraestructura`

## Reglas de dependencia

1. `CapaDominio` **no** referencia ninguna otra capa ni paquetes de infraestructura.
2. `CapaAplicacion` define interfaces (puertos) que `CapaInfraestructura` implementa (adaptadores).
3. `CapaWebApi` no contiene lógica de negocio: solo orquestación y mapeo.

## Estructura de carpetas relevante

```
CapaDominio/Entidades/
  ├─ Turnos.cs, Paciente.cs, Odontologo.cs, Tratamiento.cs, Insumos.cs
  ├─ TratamientoInsumo.cs, Usuario.cs, RefreshToken.cs
  ├─ EstadoTurno.cs, EstadoInsumo.cs, CategoriaPorInsumo.cs
  └─ ObjetosDeValor/
       ├─ IntervalosDeTiempo.cs
       └─ AgendaDiaria.cs

CapaAplicacion/
  ├─ ServiciosAplicacion/   (TurnosService, InsumosService, ...)
  ├─ DtosInput/, DtosOutput/
  ├─ Interfaces/            (puertos de repositorio y servicio)
  └─ Seguridad/             (AuthService, JwtProvider abstracción)

CapaInfraestructura/
  ├─ Implementacion.cs/     (repositorios legacy en memoria para pruebas)
  ├─ Persistencia/          (SQLite: factory, UnitOfWork, repositorios Dapper, inicializador)
  └─ Servicios/             (PasswordHasherBCrypt, JwtProvider)

CapaWebApi/
  ├─ Controllers/
  ├─ Program.cs
  └─ GlobalExceptionHandle.cs
```

## Dominio

- Modelo **rico**: entidades con setters privados y guard clauses en constructores.
- `Turno`: `Cancelar()`, `Completar()`, `Reprogramar(inicio, fin)`.
- `Insumo`: `DescontarStock(int)`, `ReponerStock(int)`, `StockDisponible()`, `RequiereReposicion()`.
- `AgendaDiaria` (Objeto de valor): encapsula turnos de un odontólogo en un día; `TieneSolapamiento`, `ObtenerHuecosDisponibles`.

## Persistencia

- `CapaInfraestructura/Persistencia/` contiene la implementación SQLite.
- `IDbConnectionFactory` (puerto) devuelve una nueva `SqliteConnection` por llamada.
- `UnitOfWorkSqlite` abre la conexión y ejecuta `BEGIN IMMEDIATE;` mediante `SqliteCommand`
  para aplicar bloqueo pesimista a nivel de archivo; confirma con `COMMIT;` y revierte con `ROLLBACK;`.
  No crea `IDbTransaction`; expone `TransaccionActiva` para que los repositorios no confirmen
  transacciones ajenas.
- `TurnoRepositorySqlite` y `TratamientoRepositorySqlite` escriben SQL explícito y
  usan Dapper (`QueryAsync<T>`, `ExecuteAsync`, `ExecuteScalarAsync`) con parámetros anónimos.
- `DatabaseInitializer` crea al arranque las tablas `Pacientes`, `Odontologos`, `Insumos`,
  `Tratamientos`, `TratamientoInsumos` y `Turnos`, con `CREATE TABLE IF NOT EXISTS`, e índices.
- Dapper mapea directamente a `Turno` y `Tratamiento` con type handlers (`DateTime` como TEXT,
  `decimal` invariante, `TimeSpan` como ticks) y type maps que fuerzan los constructores ricos;
  las invariantes del dominio se validan durante la materialización.
- Las claves foráneas se limitan a `Tratamientos`/`TratamientoInsumos` mientras `Pacientes`,
  `Odontologos` e `Insumos` sigan usando repositorios en memoria.

## Restricciones arquitectónicas del proyecto

- No introducir ORM completos ni bases de datos distintas sin especificación aprobada.
- Dapper es el micro-ORM aprobado; el SQL debe ser explícito y parametrizado.
- No agregar capas ni patrones nuevos sin justificación documentada.
- Mantener el dominio libre de dependencias externas.
- Los cambios en contratos públicos (servicios, DTOs, interfaces) requieren aprobación.

## Información desconocida / pendiente

- Estrategia de despliegue y entornos.
- Cobertura de pruebas actual.
