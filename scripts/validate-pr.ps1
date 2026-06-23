<#
.SYNOPSIS
  Aggregate PR gate: runs the architecture/oracle/test validations and checks PR hygiene.
.DESCRIPTION
  - Runs validate-clean-architecture, validate-architecture, validate-oracle-scripts, validate-tests.
  - Checks no staged secrets and no MediatR introduced (best-effort via git).
  - Verifies build with warnings-as-errors when a solution + dotnet exist.
  Exit 0 = ready for review, 1 = blocked.
#>
[CmdletBinding()]
param([string]$Root = ".")

$ErrorActionPreference = "Continue"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$failed = $false

function Step([string]$name, [scriptblock]$action) {
  Write-Host "-- $name" -ForegroundColor Cyan
  & $action
  if ($LASTEXITCODE -ne 0) { Write-Host "  [FAIL] $name failed" -ForegroundColor Red; $script:failed = $true }
  else { Write-Host "  [OK] $name" -ForegroundColor Green }
}

Step "Clean Architecture" { & "$here/validate-clean-architecture.ps1" -Root $Root }
Step "Architecture docs"  { & "$here/validate-architecture.ps1" -Root $Root }
Step "Oracle scripts"     { & "$here/validate-oracle-scripts.ps1" -Root $Root }
Step "Tests"              { & "$here/validate-tests.ps1" -Root $Root }

# git-based hygiene (best effort)
if (Get-Command git -ErrorAction SilentlyContinue) {
  $global:LASTEXITCODE = 0
  $secretLike = git -C $Root diff --cached --name-only 2>$null |
                Where-Object { $_ -match '(?i)(^|/)(\.env|.*\.pfx|.*\.pem|id_rsa|appsettings\.Secrets\.json)$' }
  if ($secretLike) { Write-Host "  [FAIL] Secret-like files staged: $($secretLike -join ', ')" -ForegroundColor Red; $failed = $true }

  $mediatr = git -C $Root diff --cached -U0 2>$null | Where-Object { $_ -match '^\+.*using\s+MediatR' }
  if ($mediatr) { Write-Host "  [FAIL] MediatR introduced in staged changes." -ForegroundColor Red; $failed = $true }
}

# build gate
$sln = Get-ChildItem -Path $Root -Recurse -Filter *.sln -ErrorAction SilentlyContinue |
       Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } | Select-Object -First 1
if ($sln -and (Get-Command dotnet -ErrorAction SilentlyContinue)) {
  Step "Build (-warnaserror)" { & dotnet build $sln.FullName -warnaserror --nologo -v quiet }
}

if ($failed) { Write-Host "`n[FAIL] PR gate failed - fix the items above before opening/merging." -ForegroundColor Red; exit 1 }
Write-Host "`n[OK] PR gate passed - ready for review." -ForegroundColor Green
exit 0
