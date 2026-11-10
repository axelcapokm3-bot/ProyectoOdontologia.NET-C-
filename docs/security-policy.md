# Política de Seguridad

El sistema odontológico maneja información personal y clínica sensible.

## Datos sensibles

- Pacientes: identidad, contacto, fecha de nacimiento.
- Historias clínicas, odontogramas, diagnósticos.
- Tratamientos, turnos, pagos.
- Usuarios, roles y permisos.
- Credenciales y tokens JWT.

## Reglas

1. **Prohibido usar datos clínicos o personales reales** en pruebas, ejemplos, demos, logs o consultas de agentes. Usar datos sintéticos.
2. No exponer valores de secretos (claves JWT, hashes, tokens) en respuestas, logs, informes ni commits.
3. Credenciales fuera del repositorio: usar User Secrets o variables de entorno.
4. `appsettings.json` solo con placeholders. Los valores reales nunca se versionan.
5. Toda entrada externa se valida en el borde (controladores/DTOs) y se revalida en el dominio (invariantes).

## Secretos y configuración

- `appsettings.json` / `appsettings.Development.json`: sin secretos reales.
- `appsettings.example.json`: plantilla con placeholders documentados (sin valores reales).
- User Secrets por entorno de desarrollo.
- Script `scripts/secret-scan.ps1`: detecta patrones de secretos y reporta **solo ubicación y tipo**, nunca el valor.

## Autorización

- JWT Bearer configurado en `CapaWebApi`.
- Roles/permisos verificados en endpoints sensibles.
- No confiar en validaciones solo del cliente.

## Revisión de seguridad

Antes de aprobar una implementación que toque entradas externas, autenticación, autorización, datos clínicos o configuración, el `reviewer` debe auditar según esta política y reportar hallazgos por severidad.
