# Plan — Autorización de Rol Paciente en CapaWebApi

## 1. Arquitectura Afectada
- Capa de presentación: `CapaWebApi` (endpoints ASP.NET Core, autorización vía `[Authorize(Roles = "...")]`).
- Capa de aplicación: `CapaAplicacion` (servicios `TurnosService`, interfaces `ITurnosService`, DTOs).
- Capa de pruebas: `CapaPruebas` (nuevos tests de integración con `WebApplicationFactory<Program>`).
- No se modifica `CapaDominio`, `CapaInfraestructura` ni controladores existentes.

## 2. Componentes y Archivos Afectados

| Archivo | Acción | Descripción |
|---|---|---|
| `CapaPruebas/ProyectoOdontologia.Tests.csproj` | Modificar | Agregar `Microsoft.AspNetCore.Mvc.Testing` para `WebApplicationFactory`. |
| `CapaPruebas/PacienteAutorizacionTests.cs` | Crear | Suite de tests de integración para el rol Paciente (13 escenarios de la spec). |

## 3. Dependencias
- `Microsoft.AspNetCore.Mvc.Testing` (NuGet) — necesario para `WebApplicationFactory<TEntryPoint>`.
- `CapaWebApi` ya referencia `CapaAplicacion`, `CapaDominio`, `CapaInfraestructura`.
- `CapaPruebas` ya referencia `CapaWebApi`, `CapaInfraestructura`, `Microsoft.AspNetCore.App`.
- `SqliteTestDatabase` (helper existente en `CapaPruebas/Persistencia/`) para base de datos temporal por prueba.

## 4. Orden de Implementación
1. Agregar paquete `Microsoft.AspNetCore.Mvc.Testing` al csproj de pruebas.
2. Crear `PacienteAutorizacionTests.cs` con `WebApplicationFactory<Program>`.
3. Implementar helper para autenticar como Paciente (registro + login + extracción de token).
4. Implementar los 13 escenarios de prueba definidos en la spec.
5. Ejecutar `dotnet build`.
6. Ejecutar `dotnet test --filter PacienteAutorizacion`.
7. Corregir fallas si las hubiera.

## 5. Riesgos
- La clave JWT está en texto plano en `appsettings.json`; los tests usarán esa misma configuración.
- `DatabaseInitializer` siembra datos al arranque; cada prueba debe usar una base de datos SQLite temporal aislada (`SqliteTestDatabase`).
- El filtrado de turnos por paciente en `ObtenerTurnosDelPacienteAsync` se hace en memoria; debe validarse con datos sintéticos.
- `WebApplicationFactory` construye el `Program.cs` real, que ejecuta `DatabaseInitializer.InicializarAsync` al arranque; se debe asegurar que no haya conflicto de archivos SQLite.

## 6. Estrategia de Pruebas
- Usar `WebApplicationFactory<Program>` para levantar un TestServer real de la API.
- Cada prueba crea su propia instancia de `SqliteTestDatabase` (archivo `.db` temporal) para aislamiento.
- Crear un usuario Paciente mediante `POST /api/Auth/registro` (o registro público), luego `POST /api/Auth/login` para obtener el JWT.
- Enviar peticiones con header `Authorization: Bearer <token>`.
- Verificar códigos de estado HTTP: `200`, `201`, `204` (permitidos) y `403` (prohibidos).
- Para anti-IDOR: crear dos pacientes, intentar cancelar/crear turnos a nombre del otro, verificar rechazo.
- No usar expresiones lambda (`=>`) para cuerpos de métodos ni consultas LINQ; usar `foreach`/`for` y `if/else`.
- No usar switch expressions en línea.
- No usar bloques `try-catch` en controladores (no se modifican controladores).
- Usar datos sintéticos únicamente.

## 7. Verificación
- `dotnet build ProyectoOdontologia.slnx` sin errores.
- `dotnet test --filter PacienteAutorizacion` en verde (100% de pruebas pasan).
- Todas las pruebas pertenecen a la categoría/filtro `PacienteAutorizacion`.

## 8. Estrategia de Reversión
- Eliminar `CapaPruebas/PacienteAutorizacionTests.cs`.
- Revertir `CapaPruebas/ProyectoOdontologia.Tests.csproj` removiendo `Microsoft.AspNetCore.Mvc.Testing`.
- No se modifican controladores ni servicios, por lo que no hay reversión de backend necesaria.
