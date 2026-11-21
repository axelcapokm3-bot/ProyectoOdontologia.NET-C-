# Spec — Autorización de Rol Paciente en CapaWebApi

## 1. Problema
El rol "Paciente" ya tiene acceso implementado en controladores y servicios de `CapaWebApi`, pero no existe una suite de pruebas de integración que verifique que el rol Paciente:
- Accede únicamente a sus endpoints permitidos.
- Recibe `403 Forbidden` en endpoints administrativos/clínicos prohibidos.
- No puede operar sobre turnos de otros pacientes (protección anti-IDOR).

## 2. Contexto
- Runtime: .NET 10, ASP.NET Core Web API.
- Autenticación: JWT Bearer con `RoleClaimType = ClaimTypes.Role`.
- Controladores: `TurnosController`, `PacienteController`, `OdontologoController`, `InsumosController`, `UsuariosController`, `TratamientoController`, `AuthController`.
- Servicios: `TurnosService` con métodos `RegistrarTurnoParaPacienteAsync`, `CancelarTurnoDelPacienteAsync`, `ObtenerTurnosDelPacienteAsync`.
- Base de datos: SQLite en memoria/archivo temporal para tests.
- No existe `PacienteAutorizacionTests.cs` ni tests con `WebApplicationFactory<Program>`.

## 3. Objetivo
Crear `PacienteAutorizacionTests.cs` usando `WebApplicationFactory<Program>` que verifique los 5 escenarios de autorización para el rol Paciente y que todos pasen al 100%.

## 4. Alcance
- Endpoints permitidos para Paciente (deben retornar 200/201):
  - `GET /api/Odontologo` (listar odontólogos para `<select>`)
  - `GET /api/Tratamiento` (listar tratamientos para `<select>`)
  - `GET /api/Turnos/mis-turnos` (turnos propios del paciente)
  - `POST /api/Turnos` (registrar turno para sí mismo)
  - `PATCH /api/Turnos/{id}/cancelar` (cancelar turno propio)
- Endpoints prohibidos para Paciente (deben retornar 403):
  - `GET /api/Turnos` (listar todos los turnos)
  - `GET /api/Paciente` (listar todos los pacientes)
  - `POST /api/Paciente` (crear pacientes)
  - `GET /api/Insumos` (listar insumos)
  - `GET /api/usuarios` (listar usuarios)
  - `PATCH /api/Turnos/{id}/completar` (completar turnos)
- Protección anti-IDOR:
  - No puede cancelar turno de otro paciente (403/409).
  - No puede crear turno a nombre de otro `PacienteId` (el servicio lo sobrescribe con su propio `PacienteId`).

## 5. Exclusiones
- No se modifican controladores ni endpoints del backend.
- No se agregan middlewares de autorización personalizados.
- No se cambian contratos públicos (DTOs, interfaces).
- No se usa `appsettings.json` con secretos reales; se configura `WebApplicationFactory` con configuración de prueba.

## 6. Requisitos Funcionales
1. `GET /api/Odontologo` con token de Paciente → `200 OK`.
2. `GET /api/Tratamiento` con token de Paciente → `200 OK`.
3. `POST /api/Turnos` con token de Paciente → `201 Created` y el turno se asocia al `PacienteId` del usuario autenticado (no al `PacienteId` del DTO).
4. `GET /api/Turnos/mis-turnos` con token de Paciente → `200 OK` y retorna únicamente los turnos de ese paciente.
5. `PATCH /api/Turnos/{id}/cancelar` con token de Paciente sobre su propio turno → `200 OK` / `204 No Content`.
6. `GET /api/Turnos` con token de Paciente → `403 Forbidden`.
7. `GET /api/Paciente` con token de Paciente → `403 Forbidden`.
8. `POST /api/Paciente` con token de Paciente → `403 Forbidden`.
9. `GET /api/Insumos` con token de Paciente → `403 Forbidden`.
10. `GET /api/usuarios` con token de Paciente → `403 Forbidden`.
11. `PATCH /api/Turnos/{id}/completar` con token de Paciente → `403 Forbidden`.
12. `PATCH /api/Turnos/{id}/cancelar` con token de Paciente sobre turno de otro paciente → `403 Forbidden` / `409 Conflict`.
13. `POST /api/Turnos` con token de Paciente y `PacienteId` de otro paciente en el DTO → se rechaza o se sobrescribe con el propio `PacienteId` (nunca opera sobre datos ajenos).

## 7. Requisitos No Funcionales
- Cero expresiones lambda (`=>`) para cuerpos de métodos o consultas LINQ en el código de prueba.
- Cero switch expressions en línea.
- Cero bloques `try-catch` en los controladores (no aplica, no se modifican controladores).
- Usar bucles explícitos (`foreach`, `for`) y bloques `if/else` tradicionales.
- Datos sintéticos únicamente (nunca datos clínicos o personales reales).
- Cada prueba debe limpiar su propia base de datos SQLite temporal.

## 8. Casos de Uso
| ID | Caso de uso | Actor | Resultado esperado |
|----|-------------|-------|-------------------|
| CU-01 | Listar odontólogos como Paciente | Paciente | 200 OK |
| CU-02 | Listar tratamientos como Paciente | Paciente | 200 OK |
| CU-03 | Registrar turno propio como Paciente | Paciente | 201 Created, PacienteId propio |
| CU-04 | Listar mis turnos como Paciente | Paciente | 200 OK, solo turnos propios |
| CU-05 | Cancelar turno propio como Paciente | Paciente | 200 / 204 |
| CU-06 | Acceso denegado a listar todos los turnos | Paciente | 403 |
| CU-07 | Acceso denegado a listar pacientes | Paciente | 403 |
| CU-08 | Acceso denegado a crear pacientes | Paciente | 403 |
| CU-09 | Acceso denegado a listar insumos | Paciente | 403 |
| CU-10 | Acceso denegado a listar usuarios | Paciente | 403 |
| CU-11 | Acceso denegado a completar turno | Paciente | 403 |
| CU-12 | Anti-IDOR: cancelar turno ajeno | Paciente | 403 / 409 |
| CU-13 | Anti-IDOR: crear turno para otro paciente | Paciente | Rechazado o sobrescrito con propio PacienteId |

## 9. Criterios de Aceptación
- [ ] `dotnet build ProyectoOdontologia.slnx` sin errores.
- [ ] `dotnet test --filter PacienteAutorizacion` en verde (100% de pruebas pasan).
- [ ] Todas las pruebas usan `WebApplicationFactory<Program>` (integración real de API).
- [ ] No se modifican controladores ni endpoints del backend.
- [ ] No se usan datos clínicos o personales reales.

## 10. Restricciones
- No introducir ORM ni bases de datos distintas.
- No agregar capas ni patrones nuevos.
- Mantener el dominio libre de dependencias externas.
- No cambiar contratos públicos sin aprobación.

## 11. Riesgos
- La clave JWT está en `appsettings.json` (texto plano). Para los tests se usará la misma configuración del archivo.
- `DatabaseInitializer` siembra al arranque; cada prueba debe usar una base de datos temporal limpia.
- El filtrado de turnos por paciente en `ObtenerTurnosDelPacienteAsync` se hace en memoria, no en SQL.

## 12. Decisiones Pendientes
Ninguna.

## 13. Registro de Aprobaciones
- Aprobado por el usuario para ejecutar directamente (modo autónomo).
