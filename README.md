# Proyecto Odontología

Sistema de gestión odontológica con backend ASP.NET Core Web API y frontend estático.

## Stack

- .NET 10 / ASP.NET Core Web API
- Clean Architecture / Arquitectura Cebolla
- Dapper + Microsoft.Data.Sqlite (SQL explícito)
- JWT Bearer + BCrypt
- xUnit + Moq
- Frontend HTML/CSS/JavaScript en `CapaDePresentacion`

## Estructura

| Capa | Responsabilidad |
| --- | --- |
| `CapaDominio` | Entidades, objetos de valor e invariantes. Sin dependencias externas. |
| `CapaAplicacion` | Casos de uso, DTOs e interfaces de repositorio/servicio. |
| `CapaInfraestructura` | Repositorios Dapper/SQLite, Unit of Work, BCrypt y JWT. |
| `CapaWebApi` | Controladores, middleware de errores, DI y configuración. |
| `CapaPruebas` | Pruebas unitarias, de persistencia y de Web API. |
| `CapaDePresentacion` | Interfaz web estática que consume la API. |

## Requisitos

- SDK .NET 10
- Navegador moderno

## Ejecución local

1. Restaurar y compilar:

```powershell
dotnet build ProyectoOdontologia.slnx
```

2. Levantar la API:

```powershell
dotnet run --project CapaWebApi
```

La API queda disponible en `http://localhost:5231` y Swagger en
`http://localhost:5231/swagger` cuando el entorno es `Development`.

3. Abrir el frontend:

- Abrir `CapaDePresentacion/Login.html` directamente, o
- Servir la carpeta `CapaDePresentacion` con un servidor estático y abrir `Login.html`.

La URL de la API se configura en `CapaDePresentacion/assets/js/config.js` o desde
el botón de configuración del frontend.

## Usuario de demo

El backend solo siembra el usuario administrador local si la contraseña está
configurada en User Secrets (nunca en el repositorio):

```powershell
dotnet user-secrets init --project CapaWebApi
dotnet user-secrets set "Seed:AdminPassword" "<contraseña-local-minimo-8-caracteres>" --project CapaWebApi
```

Si `Seed:AdminPassword` no está configurada, el seed se omite y se registra una
advertencia al arrancar. No usar credenciales reales ni datos clínicos reales en la demo.

## Configuración de secretos

`CapaWebApi/appsettings.json` contiene solo placeholders. Para un entorno local o
compartido, configurar la clave JWT con User Secrets:

```powershell
dotnet user-secrets init --project CapaWebApi
dotnet user-secrets set "Jwt:Key" "<clave-local-de-al-menos-32-caracteres>" --project CapaWebApi
```

No versionar claves, tokens ni contraseñas reales.

## Base de datos

La API crea `clinica.db` y aplica el esquema al iniciar. El archivo está ignorado
por git. Para reiniciar la demo, detener la API y renombrar o eliminar
`clinica.db`, `clinica.db-shm` y `clinica.db-wal`.

Datos creados por el esquema: pacientes, odontólogos, insumos, tratamientos,
relaciones tratamiento-insumo, movimientos de stock y turnos.

## Flujo de demo sugerido

1. Iniciar sesión con el usuario administrador local.
2. Crear un insumo con stock suficiente.
3. Crear un paciente y un odontólogo.
4. Crear un tratamiento con duración y al menos un insumo requerido.
5. Crear un turno para ese paciente, odontólogo y tratamiento.
6. Completar el turno y verificar el descuento de stock en Insumos.
7. Cancelar otro turno y verificar el cambio de estado.
8. Probar búsqueda, filtros y paginación.
9. Reiniciar la API y confirmar que los datos persisten.

Usar únicamente datos sintéticos.

## Endpoints principales

| Método | Ruta | Descripción |
| --- | --- | --- |
| POST | `/api/Auth/login` | Inicio de sesión y emisión de JWT. |
| POST | `/api/Auth/register` | Registro de usuario. |
| POST | `/api/Auth/refresh` | Renovación de token. |
| GET/POST/PUT/DELETE | `/api/Paciente` | CRUD de pacientes. |
| GET/POST/PUT/DELETE | `/api/Odontologo` | CRUD de odontólogos. |
| GET/POST/PUT/DELETE | `/api/Tratamiento` | CRUD de tratamientos e insumos requeridos. |
| GET/POST/PUT/DELETE | `/api/Insumos` | CRUD y control de stock. |
| GET/POST/PUT/DELETE | `/api/Turnos` | Agenda de turnos. |
| PATCH | `/api/Turnos/{id}/cancelar` | Cancela un turno. |
| PATCH | `/api/Turnos/{id}/completar` | Completa un turno y descuenta insumos. |
| GET | `/api/Turnos/huecos` | Consulta huecos disponibles. |

## Verificación

```powershell
dotnet build ProyectoOdontologia.slnx
dotnet test CapaPruebas\ProyectoOdontologia.Tests.csproj
powershell -ExecutionPolicy Bypass -File scripts/secret-scan.ps1
```

## Reglas del proyecto

- No usar datos clínicos o personales reales.
- No versionar secretos.
- No introducir ORM completos, capas o dependencias sin especificación aprobada.
- Mantener las invariantes en el dominio y el SQL parametrizado.
