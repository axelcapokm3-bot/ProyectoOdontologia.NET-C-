<#
.SYNOPSIS
  Detecta posibles secretos en el repositorio.
.DESCRIPTION
  Escanea archivos de texto buscando patrones de secretos comunes y reporta
  UNICAMENTE la ubicación y el tipo de hallazgo. Nunca imprime el valor.
#>
param(
    [string]$Root = (Get-Location).Path,
    [string[]]$ExcludeDirs = @('.git', 'bin', 'obj', 'node_modules', '.vs', 'DeepTutor')
)

$patterns = @(
    @{ Name = 'JWT asignado';        Regex = 'Jwt\s*:\s*\{[^}]*Key\s*[:=]\s*"[^"]{16,}"' },
    @{ Name = 'JWT Key';             Regex = '(?i)"Key"\s*:\s*"[^"]{8,}"' },
    @{ Name = 'Clave/Token genérico'; Regex = '(?i)(api[_-]?key|secret|password|passwd|token|connectionstring)\s*[:=]\s*"[^"\s]{8,}"' },
    @{ Name = 'Bearer token';         Regex = 'Bearer\s+[A-Za-z0-9\-_\.]{20,}' },
    @{ Name = 'Clave privada';        Regex = '-----BEGIN (RSA |EC |OPENSSH )?PRIVATE KEY-----' },
    @{ Name = 'AWS Access Key';       Regex = 'AKIA[0-9A-Z]{16}' },
    @{ Name = 'Cadena de conexión';   Regex = '(?i)(Server|Data Source)\s*=\s*[^;]+;\s*(Database|Initial Catalog)\s*=' }
)

$placeholderHints = @('TU_CLAVE', 'PLACEHOLDER', 'EXAMPLE', 'CHANGEME', 'CHANGE_ME', 'XXXX')

$files = Get-ChildItem -Path $Root -Recurse -File -ErrorAction SilentlyContinue |
    Where-Object {
        $rel = $_.FullName.Substring($Root.Length).TrimStart('\', '/')
        $excluded = $false
        foreach ($d in $ExcludeDirs) {
            if ($rel -match [regex]::Escape($d + [IO.Path]::DirectorySeparatorChar) -or $rel -like "$d*") { $excluded = $true; break }
        }
        -not $excluded -and $_.Length -lt 2MB -and
        $_.Extension -in @('.cs', '.json', '.jsonc', '.config', '.xml', '.yml', '.yaml', '.csproj', '.md', '.txt', '.ps1', '.env', '.sh')
    }

$findings = 0
foreach ($file in $files) {
    $rel = $file.FullName.Substring($Root.Length).TrimStart('\', '/')
    $lineNo = 0
    foreach ($line in Get-Content -LiteralPath $file.FullName -ErrorAction SilentlyContinue) {
        $lineNo++
        foreach ($p in $patterns) {
            if ($line -match $p.Regex) {
                $isPlaceholder = $false
                foreach ($h in $placeholderHints) {
                    if ($line.ToUpper().Contains($h)) { $isPlaceholder = $true; break }
                }
                $tag = if ($isPlaceholder) { 'PLACEHOLDER' } else { 'POSIBLE SECRETO' }
                if (-not $isPlaceholder) { $findings++ }
                Write-Output ("[{0}] {1}:{2} - {3}" -f $tag, $rel, $lineNo, $p.Name)
            }
        }
    }
}

if ($findings -eq 0) {
    Write-Output "OK: no se detectaron secretos reales (los placeholders se marcan aparte)."
} else {
    Write-Output ("ADVERTENCIA: {0} posible(s) secreto(s) detectado(s). No se imprimió ningún valor." -f $findings)
    exit 1
}
