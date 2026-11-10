# Flujo de Desarrollo SDD

## Flujo obligatorio

```
IDEA → EXPLORACIÓN → ANÁLISIS → PREGUNTAS → DECISIÓN DEL USUARIO
     → SPEC → PLAN → TASKS → IMPLEMENTACIÓN → TESTS → REVIEW
     → VALIDACIÓN → MEMORY
```

Ninguna fase se saltea. Ninguna tarea se implementa sin estar aprobada.

## Roles

| Fase | Agente | Artefacto |
| --- | --- | --- |
| Exploración / análisis | `explore` | informe de hallazgos |
| Spec / plan / tasks | `planner` | `spec/NNN-*/` |
| Implementación / tests | `implementer` | código + pruebas |
| Revisión | `reviewer` | informe de review |
| Coordinación / aprobaciones | `coordinator` | decisiones |

## Spec (`spec.md`)

Debe contener: Problema, Contexto, Objetivo, Alcance, Exclusiones, Requisitos funcionales, Requisitos no funcionales, Reglas de negocio, Casos de uso, Criterios de aceptación, Restricciones, Riesgos, Decisiones pendientes, Registro de aprobaciones.

## Plan (`plan.md`)

Arquitectura afectada, componentes, archivos afectados, dependencias, orden de implementación, riesgos, estrategia de pruebas, verificación, estrategia de reversión.

## Tasks (`tasks.md`)

Cada tarea: identificador, descripción, archivos involucrados, dependencias, resultado esperado, pruebas necesarias, estado.

## Definición de terminado (DoD)

- [ ] Spec aprobada por el usuario.
- [ ] Plan y tasks aprobados.
- [ ] `dotnet build` sin errores.
- [ ] `dotnet test` en verde con pruebas del nuevo comportamiento.
- [ ] Review del `reviewer` sin hallazgos críticos/altos abiertos.
- [ ] Decisión/aprendizaje relevante registrado en `memory/`.

## Comandos de verificación

```powershell
dotnet build ProyectoOdontologia.slnx
dotnet test  ProyectoOdontologia.slnx
powershell -File scripts/secret-scan.ps1
```
