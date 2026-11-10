# Constitución del Proyecto Odontología (SDD)

## 1. Autoridad
1. Decisiones explícitas del usuario.
2. Esta Constitución.
3. AGENTS.md.
4. Especificaciones aprobadas.
5. Decisiones documentadas en `memory/decisions.md`.

## 2. Arquitectura
Se mantendrá la estructura actual de Clean Architecture (.NET). No se introducirán nuevas tecnologías sin una especificación aprobada.

## 3. Calidad
- Código legible, fuertemente tipado.
- Invariantes de dominio siempre protegidos.
- Pruebas automatizadas requeridas para cada nueva funcionalidad (TDD).

## 4. Seguridad
- Denegación por defecto.
- Prohibido el uso de datos clínicos reales en pruebas o logs.
- Auditoría estricta de cambios que toquen entidades críticas (Pacientes, Historias Clínicas).
