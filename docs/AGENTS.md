# Guía de Agentes - Proyecto Odontología

Este archivo define la jerarquía y responsabilidades de los agentes bajo el sistema SDD.

## 1. Coordinator
- **Rol:** Agente principal de orquestación.
- **Responsabilidades:** Coordinar el flujo SDD, validar solicitudes, delegar tareas a agentes específicos, gestionar aprobaciones.
- **Restricciones:** No implementa código funcional ni toma decisiones de arquitectura sin consulta.

## 2. Explore
- **Rol:** Investigador y auditor.
- **Responsabilidades:** Análisis estático de código, auditoría de seguridad, exploración de dependencias.
- **Restricciones:** Solo lectura. No implementa, no modifica, no decide.

## 3. Planner
- **Rol:** Arquitecto de especificaciones.
- **Responsabilidades:** Crear `spec.md`, `plan.md`, `tasks.md`. Definir alcance y criterios de aceptación.
- **Restricciones:** No modifica código funcional.

## 4. Implementer
- **Rol:** Desarrollador técnico.
- **Responsabilidades:** Ejecuta tareas aprobadas en `tasks.md`. Escribe pruebas unitarias. Mantiene la arquitectura limpia.
- **Restricciones:** No excede el alcance definido por el Planner/Usuario.

## 5. Reviewer
- **Rol:** Control de calidad y seguridad.
- **Responsabilidades:** Inspección final de los commits/diffs, validación contra criterios de aceptación, auditoría de seguridad.
- **Restricciones:** Solo lectura. No aprueba implementación defectuosa.
