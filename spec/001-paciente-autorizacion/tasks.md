# Tasks — Autorización de Rol Paciente en CapaWebApi

## T-01: Agregar Microsoft.AspNetCore.Mvc.Testing al csproj de pruebas
- **Archivo:** `CapaPruebas/ProyectoOdontologia.Tests.csproj`
- **Descripción:** Agregar `<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.0" />` para poder usar `WebApplicationFactory<Program>`.
- **Dependencias:** Ninguna.
- **Resultado esperado:** El csproj compila y el paquete está disponible para los tests de integración.
- **Pruebas necesarias:** `dotnet build` sin errores.
- **Estado:** pendiente

## T-02: Crear PacienteAutorizacionTests.cs
- **Archivo:** `CapaPruebas/PacienteAutorizacionTests.cs`
- **Descripción:** Suite de tests de integración usando `WebApplicationFactory<Program>` que verifica los 13 escenarios de la spec (permitidos, prohibidos, anti-IDOR).
- **Dependencias:** T-01.
- **Resultado esperado:** Archivo de tests creado con todos los escenarios implementados sin lambdas, sin switch expressions, sin try-catch en controladores.
- **Pruebas necesarias:** `dotnet test --filter PacienteAutorizacion` en verde.
- **Estado:** pendiente

## T-03: Ejecutar build y tests
- **Archivo:** `ProyectoOdontologia.slnx`
- **Descripción:** Ejecutar `dotnet build` y `dotnet test --filter PacienteAutorizacion`.
- **Dependencias:** T-01, T-02.
- **Resultado esperado:** Build sin errores, 100% de pruebas pasan.
- **Pruebas necesarias:** Verificación de salida de `dotnet test`.
- **Estado:** pendiente

## T-04: Review y reporte final
- **Archivo:** Todos los archivos modificados/creados.
- **Descripción:** Revisar diffs, verificar cumplimiento de restricciones de sintaxis C# y entregable de reporte.
- **Dependencias:** T-03.
- **Resultado esperado:** Informe de revisión sin hallazgos críticos.
- **Pruebas necesarias:** Ninguna (revisión manual).
- **Estado:** pendiente
