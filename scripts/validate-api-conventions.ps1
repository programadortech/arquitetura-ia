<#
.SYNOPSIS
  Verifica convencoes da camada de API (ADR-0028 / docs/standards/api-layer.md). Exit 0 = pass, 1 = violacoes.
.DESCRIPTION
  Checagens objetivas (o julgamento fino de SRP/status code fica para a revisao):
    1. Program.cs enxuto (<= limite de linhas de codigo).
    2. Controllers/Endpoints sem logica indevida (sem DbContext/UserManager/RoleManager/new SmtpClient/new HttpClient).
    3. Composicao presente: existe pasta Extensions/ e o app mapeia (MapControllers OU Map*Endpoints).
  Sem projeto Api (template recem-clonado) -> nada a validar (exit 0).
#>
[CmdletBinding()]
param([string]$Root = ".", [int]$MaxProgramLines = 60)

$ErrorActionPreference = "Stop"
$violations = New-Object System.Collections.Generic.List[string]

$apiProj = Get-ChildItem -Path $Root -Recurse -Filter *.Api.csproj -ErrorAction SilentlyContinue |
           Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' -and $_.FullName -notmatch '[\\/]building-blocks[\\/]' } | Select-Object -First 1
if (-not $apiProj) {
  Write-Host "[INFO] nenhum projeto *.Api (nada a validar)." -ForegroundColor Yellow
  exit 0
}
$apiDir = $apiProj.Directory.FullName

# 1) Program.cs enxuto
$program = Join-Path $apiDir "Program.cs"
if (Test-Path $program) {
  $code = @(Get-Content $program | Where-Object { $_.Trim() -ne "" -and -not $_.Trim().StartsWith("//") })
  if ($code.Count -gt $MaxProgramLines) {
    $violations.Add("Program.cs tem $($code.Count) linhas de codigo (> $MaxProgramLines). Extraia a composicao para Extensions/ (ADR-0028).")
  }
}

# 2) Controllers/Endpoints finos
$forbidden = @('DbContext', 'UserManager<', 'RoleManager<', 'new SmtpClient', 'new HttpClient', 'IDbConnection')
$apiFiles = Get-ChildItem -Path $apiDir -Recurse -Filter *.cs -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' }
$borderFiles = $apiFiles | Where-Object { $_.Name.EndsWith("Controller.cs") -or $_.FullName -match '[\\/]Endpoints[\\/]' }
foreach ($f in $borderFiles) {
  $content = Get-Content $f.FullName -Raw
  foreach ($p in $forbidden) {
    if ($content.Contains($p)) {
      $violations.Add("Borda da API com logica indevida ('$p') em $($f.Name) - controller/endpoint deve so despachar (SRP, ADR-0028).")
    }
  }
}

# 2b) Controllers nao declaram contratos (request/response) — devem ficar em Contracts/ (ADR-0028).
$controllerFiles = $apiFiles | Where-Object { $_.Name.EndsWith("Controller.cs") }
foreach ($f in $controllerFiles) {
  if ((Get-Content $f.FullName -Raw) -match '\brecord\s+\w+\s*\(') {
    $violations.Add("Contrato (record) declarado dentro de $($f.Name) - mova request/response para Contracts/<Recurso>/ (ADR-0028).")
  }
}

# 3) Composicao: pasta Extensions/ e mapeamento presente
if (-not (Test-Path (Join-Path $apiDir "Extensions"))) {
  $violations.Add("Faltou a pasta Extensions/ na API (composicao do Program em extension methods - ADR-0028).")
}
$allApi = ($apiFiles | Get-Content -Raw) -join "`n"
if (-not $allApi.Contains("MapControllers(") -and $allApi -notmatch 'Map\w+Endpoints\(') {
  $violations.Add("A API nao mapeia endpoints (esperado MapControllers() ou Map*Endpoints()).")
}

if ($violations.Count -gt 0) {
  Write-Host "[FAIL] Convencoes da camada de API violadas:" -ForegroundColor Red
  $violations | ForEach-Object { Write-Host "   - $_" -ForegroundColor Red }
  exit 1
}
Write-Host "[OK] Convencoes da camada de API satisfeitas." -ForegroundColor Green
exit 0
