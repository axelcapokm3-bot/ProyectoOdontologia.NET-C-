# MCP — Inventario y Decisiones

> Auditoría al 2026-11-09. Estado: **no se instaló ningún servidor MCP**.

## Verificado
- El proyecto **no** usa ORM ni motor de base de datos: los `Repositorio*` de `CapaInfraestructura/Implementacion.cs/` operan en memoria.
- No existe configuración MCP previa en el repositorio (no había `.opencode/`).
- `appsettings.json` solo contiene un placeholder de clave JWT.

## MCP de base de datos
- **No aplica** en la actualidad: no hay motor de base de datos que consultar.
- Si en el futuro se introduce persistencia (ver `docs/architecture.md` → pendiente), evaluar un MCP de BD con:
  - Cuenta exclusiva para MCP, solo entorno de desarrollo.
  - Permisos mínimos, solo lectura por defecto, DML/DDL deshabilitados.
  - Timeouts y límites de consultas.
  - Credenciales fuera del repositorio (variables de entorno / `{env:VAR}`).
  - Prohibido consultar datos clínicos/personales reales; usar esquemas, metadatos y datos sintéticos.
- **Decisión:** no habilitar hasta que exista el motor y se cumplan las restricciones.

## Otros MCP evaluados
| Necesidad | Recomendación | Estado |
| --- | --- | --- |
| Documentación .NET | No requerido hoy; el SDK local basta. | No instalado |
| Navegador / pruebas visuales | No hay frontend propio en este repo (la UI es HTML en `CapaDePresentacion`). | No instalado |
| Diseño de interfaces | No aplica al backend. | No instalado |

## Regla de seguridad
Nunca almacenar contraseñas, tokens ni claves API en archivos versionados. Si se agrega un MCP, su configuración va en `.opencode/opencode.json` con interpolación `{env:VAR}` y las credenciales en el entorno del usuario.

## Requiere aprobación
Cualquier instalación futura de MCP requiere aprobación explícita del usuario.
