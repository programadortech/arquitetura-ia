<#
.SYNOPSIS
  Validates the Clean Architecture dependency rule across the solution(s) by inspecting
  project references and using statements. Exit 0 = pass, 1 = violations found.
.DESCRIPTION
  Static, build-free checks (the *.ArchitectureTests project is the authoritative runtime gate;
  this script is the fast pre-PR / hook check):
    1. Domain has no ProjectReference.
    2. Application references only Domain.
    3. Application has no 'using *.Infrastructure'.
    4. Infrastructure does not reference Api.
    5. No source file references MediatR.
#>
[CmdletBinding()]
param([string]$Root = ".")

$ErrorActionPreference = "Stop"
$violations = New-Object System.Collections.Generic.List[string]
function Fail([string]$m) { $script:violations.Add($m) }

$csprojs = Get-ChildItem -Path $Root -Recurse -Filter *.csproj -ErrorAction SilentlyContinue
if (-not $csprojs) {
  Write-Host "[INFO] no .csproj found (template repo has no generated project yet) - nothing to validate." -ForegroundColor Yellow
  exit 0
}

foreach ($proj in $csprojs) {
  $name = $proj.BaseName
  $xml  = [xml](Get-Content $proj.FullName)
  $refs = @($xml.Project.ItemGroup.ProjectReference.Include | Where-Object { $_ }) |
          ForEach-Object { [System.IO.Path]::GetFileNameWithoutExtension($_) }

  if ($name -match '\.Domain$' -and $refs.Count -gt 0) {
    Fail "Domain project '$name' must have NO project references. Found: $($refs -join ', ')"
  }
  if ($name -match '\.Application$' -and $name -notmatch '^BuildingBlocks\.') {
    foreach ($r in $refs) {
      if ($r -notmatch '\.Domain$' -and $r -notmatch '^BuildingBlocks\.') {
        Fail "Application project '$name' references '$r' - it may reference only *.Domain or BuildingBlocks.*."
      }
    }
  }
  if ($name -match '\.Infrastructure$') {
    foreach ($r in $refs) {
      if ($r -match '\.Api$') { Fail "Infrastructure '$name' must not reference Api ('$r')." }
    }
  }
}

# using-statement & MediatR checks
$csFiles = Get-ChildItem -Path $Root -Recurse -Filter *.cs -ErrorAction SilentlyContinue |
           Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' }
foreach ($f in $csFiles) {
  $content = Get-Content $f.FullName -Raw
  if ($f.FullName -match '\.Application[\\/]' -and $content -match 'using\s+[\w\.]*\.Infrastructure') {
    Fail "Application file leaks Infrastructure: $($f.FullName)"
  }
  if ($content -match 'using\s+MediatR') {
    Fail "MediatR is forbidden (use IUseCaseDispatcher): $($f.FullName)"
  }
}

if ($violations.Count -gt 0) {
  Write-Host "[FAIL] Clean Architecture violations:" -ForegroundColor Red
  $violations | ForEach-Object { Write-Host "   - $_" -ForegroundColor Red }
  exit 1
}
Write-Host "[OK] Clean Architecture rules satisfied." -ForegroundColor Green
exit 0
