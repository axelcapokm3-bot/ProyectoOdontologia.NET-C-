# Lecciones Aprendidas

Aprendizajes para evitar errores recurrentes. Breves y accionables.

- **Dominio:** Las entidades deben proteger invariantes en el constructor; nunca validar solo en servicios.
- **AgendaDiaria:** Si la fecha es hoy, el cálculo de huecos debe avanzar el puntero a `DateTime.Now` para no ofrecer franjas pasadas.
- **Insumos:** `DescontarStock` usa stock disponible (`Stock - StockReservado`), no solo `Stock`, para evitar sobreventa.
- **Config OpenCode:** El objeto de agentes es `agent` (singular) y los permisos son `permission`; validar contra `https://opencode.ai/config.json` antes de guardar.
- **Contratos públicos:** Al endurecer el dominio (p. ej. `private set`), revisar consumidores en `CapaInfraestructura`/`CapaAplicacion`. Conservar propiedades calculadas usadas externamente (`Turno.FechaHora`) y adaptar llamadas a métodos renombrados (`DescontarStock`).
- **Namespaces:** Los objetos de valor viven en `CapaDominio.ObjetosDeValor` (no `CapaDominio.Entidades.ObjetosDeValor`), pese a la carpeta física.
- **Build/Test:** `dotnet build ProyectoOdontologia.slnx` verde; `dotnet test CapaPruebas\ProyectoOdontologia.Tests.csproj` verde al cierre de la persistencia con Dapper.
- **Dapper + dominio rico:** no depender del mapeo automático de constructores privados. Usar una fila DTO simple (`TurnoRow`, `TratamientoRow`) y materializar con el constructor rico; así se validan invariantes y no se abren setters.
- **Dapper + SQLite:** escuchar `last_insert_rowid()` en la misma conexión/transacción que el `INSERT`; es un valor por conexión. Si el ID se preasigna, validar que el valor retornado coincida.
- **Dapper + transacciones:** `IUnitOfWork` debe exponer la misma conexión y transacción que usan los repositorios; una conexión nueva por repositorio quedaría fuera de la transacción. El factory crea conexiones nuevas y el UoW las comparte.
- **SQLite + fechas:** persistir `DateTime` como TEXT con formato fijo `yyyy-MM-dd HH:mm:ss` (invariante) para que `DATE()`, `strftime()` y el orden lexicográfico funcionen sin depender de la cultura.
- **SQLite + dinero:** persistir `decimal` como TEXT invariante y parsearlo explícitamente; `Convert` con cultura local puede interpretar mal el separador decimal.
- **SQLite + FK:** activar `Foreign Keys=True` en la cadena de conexión para que `ON DELETE CASCADE` funcione en cada conexión.
- **EF Core (histórico):** descartado por decisión del usuario; no reintroducir `DbContext` ni paquetes `Microsoft.EntityFrameworkCore.*` sin aprobación.
- **Overloads y pruebas:** al agregar sobrecargas, `GetMethod(nameof(...))` lanza ambigüedad y `null!` no compila; especificar `Type[]` y castear el `null`.
- **Namespaces en pruebas:** dentro de `namespace ProyectoOdontologia.*`, `using CapaAplicacion...` se resuelve relativo; usar `using global::CapaAplicacion...`.
- **Dapper y encapsulamiento:** por defecto Dapper prefiere un constructor sin parámetros (aunque sea privado) y asigna setters privados, omitiendo invariantes. Para modelos ricos, registrar un `ITypeMap` que devuelva el constructor parametrizado y type handlers para `DateTime`, `decimal` y `TimeSpan`.
- **Dapper y firmas de constructor:** la selección de constructor usa los tipos crudos del reader (p. ej. `Int64`, `String`), no los tipos de los type handlers. El `ITypeMap` debe forzar el constructor; la conversión por columna la resuelve Dapper después.
- **SQLite y transacción explícita:** si se ejecuta `BEGIN IMMEDIATE` como SQL, no existe `IDbTransaction`; los repositorios deben consultar `TransaccionActiva` y no confiar en `Transaction is null` para decidir si inician/confirman una transacción propia.
- **SQLite y concurrencia:** `BEGIN IMMEDIATE` bloquea escritores concurrentes; el `Default Timeout` de la cadena de conexión define cuánto espera antes de `SQLITE_BUSY`. Probar con timeout corto en segunda conexión.
- **DDL y repos en memoria:** no declarar FK hacia tablas cuyos repositorios siguen en memoria (Pacientes, Odontólogos, Insumos) hasta migrarlos; de lo contrario fallan las inserciones de turnos/tratamientos con IDs no persistidos.
- **Dapper + SQLite + constructores:** el reader entrega `Int64` y un `int?` en el constructor puede disparar `InvalidProgramException` o el error de firma. Para `Paciente`/`Odontologo`, mapear a una fila DTO interna y convertir con `checked((int)...)`; reservar los type maps para entidades sin parámetros nullable problemáticos (`Turno`, `Tratamiento`).
- **AgendaDiaria:** al validar un turno nuevo, no retornar apenas se encuentra un hueco válido; seguir recorriendo la agenda para detectar solapamientos preexistentes entre turnos.
- **Middleware ASP.NET Core:** `WriteAsJsonAsync` con `contentType` y `cancellationToken` requiere pasar también `options: null`; de lo contrario no resuelve la sobrecarga.
- **Transacciones y errores de dominio:** con `BeginTransactionAsync` al inicio, no hace falta `catch` en el servicio; el `DisposeAsync` del UoW revierte la transacción abierta. En pruebas de integración, usar un UoW nuevo por llamada para no reutilizar una transacción fallida.
- **Pruebas de middleware:** `DefaultHttpContext` + `NullLogger<T>` + `JsonSerializer.DeserializeAsync<ProblemDetails>` permiten validar estado, content type y detalle sin servidor; no exponer el mensaje de una excepción no controlada en el 500.
- **Insumos y stock:** reconstruir `Insumo` con el constructor completo para conservar `StockReservado`, `PuntoPedido`, `StockSeguridad` y `StockMaximo`; usar `Actualizar` para editar sin reiniciar esos valores.
- **Auditoría de stock:** el `UPDATE Insumos` y el `INSERT MovimientosStock` deben compartir la misma transacción `BEGIN IMMEDIATE`; el rollback debe revertir ambos.
- **FK con datos existentes:** `CREATE TABLE IF NOT EXISTS` no agrega FK a tablas ya creadas. En bases previas a esta fase hay que recrear/migrar `TratamientoInsumos` para obtener `InsumoId ON DELETE RESTRICT`.
- **Pruebas de repositorios con FK:** sembrar los insumos antes de insertar `TratamientoInsumos`; usar `SqliteException.SqliteErrorCode == 19` para verificar la restricción.
- **Usuarios fuera de SQLite:** las altas automáticas de usuario en `PacienteService`/`OdontologoService` no participan de la transacción SQLite; no prometer atomicidad entre usuario y paciente/odontólogo.
