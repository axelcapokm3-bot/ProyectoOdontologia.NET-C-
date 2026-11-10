---
name: privacy-and-secret-audit
description: Use when auditing this odontología repo for exposed secrets, JWT keys, credentials, or clinical/personal data leakage in code, config, logs or tests. Triggers on "auditoría de seguridad", "secretos", config changes, auth changes, or before merging anything touching Paciente/historia clínica.
---

# Auditoría de Privacidad y Secretos

El sistema maneja datos clínicos y personales. Aplicá estas comprobaciones.

## Secretos
- ¿Hay claves JWT, passwords, tokens o cadenas de conexión reales versionadas?
- `appsettings*.json` debe usar placeholders; los reales van a User Secrets / env.
- Ejecutá `scripts/secret-scan.ps1` y revisá los hallazgos `POSIBLE SECRETO`.
- **Nunca** imprimas el valor del secreto: reportá solo `archivo:línea` y tipo.

## Datos personales/clínicos
- ¿Se usan datos reales en pruebas, ejemplos, seeds o logs? → Prohibido.
- ¿Se loguean identificadores, diagnósticos, odontogramas o contactos?
- ¿Las respuestas de error filtran datos internos?

## Autenticación y autorización
- JWT Bearer: validación de issuer/audience/expiración.
- Endpoints sensibles con `[Authorize]` y roles correctos.
- No confiar en validación solo del cliente.

## Entradas externas
- Validación en el borde (DTO/controlador) y revalidación de invariantes en dominio.
- Prevención de inyección si en el futuro se agrega persistencia.

## Salida
Hallazgos por severidad con `archivo:línea`, impacto y recomendación. Sin exponer valores sensibles.
