<#
.SYNOPSIS
  Verificación de calidad del proyecto: build, tests y escaneo de secretos.
#>
param(
    [switch]$SkipTests
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

Write-Host "==> Escaneo de secretos" -ForegroundColor Cyan
& (Join-Path $PSScriptRoot 'secret-scan.ps1') -Root $root

Write-Host "==> Build" -ForegroundColor Cyan
dotnet build (Join-Path $root 'ProyectoOdontologia.slnx')
if ($LASTEXITCODE -ne 0) { throw "Build falló." }

if (-not $SkipTests) {
    Write-Host "==> Tests" -ForegroundColor Cyan
    dotnet test (Join-Path $root 'ProyectoOdontologia.slnx')
    if ($LASTEXITCODE -ne 0) { throw "Tests fallaron." }
}

Write-Host "Verificación completada." -ForegroundColor Green
