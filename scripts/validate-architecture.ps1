<#
.SYNOPSIS
  Validates that required architecture documentation exists and is consistent.
.DESCRIPTION
  Checks:
    1. Core standards docs exist.
    2. The ADR directory exists and ADRs are sequentially numbered without gaps.
    3. Every feature in docs/features has a matching architecture doc (warn-only).
  Exit 0 = pass, 1 = missing required artifacts.
#>
[CmdletBinding()]
param([string]$Root = ".")

$ErrorActionPreference = "Stop"
$errors = New-Object System.Collections.Generic.List[string]
$warns  = New-Object System.Collections.Generic.List[string]

$required = @(
  "docs/standards/architecture.md",
  "docs/standards/usecase-dispatcher.md",
  "docs/standards/oracle.md",
  "docs/standards/observability.md",
  "docs/standards/resilience.md",
  "docs/standards/queue-providers.md",
  "docs/standards/jobs.md",
  "docs/standards/testing.md",
  "docs/standards/quality-checklist.md"
)
foreach ($r in $required) {
  if (-not (Test-Path (Join-Path $Root $r))) { $errors.Add("Missing required standard: $r") }
}

# ADR numbering
$adrDir = Join-Path $Root "docs/adr"
if (-not (Test-Path $adrDir)) {
  $errors.Add("Missing docs/adr directory.")
} else {
  $adrs = Get-ChildItem $adrDir -Filter "*.md" |
          Where-Object { $_.Name -match '^\d{4}-' } |
          Sort-Object Name
  $expected = 1
  foreach ($a in $adrs) {
    $num = [int]($a.Name.Substring(0,4))
    if ($num -ne $expected) { $errors.Add("ADR numbering gap: expected $($expected.ToString('0000')), found $($a.Name)") }
    $expected++
  }
}

# Feature -> architecture coverage (warn only)
$featuresDir = Join-Path $Root "docs/features"
$archDir     = Join-Path $Root "docs/architecture"
if (Test-Path $featuresDir) {
  Get-ChildItem $featuresDir -Filter "*.md" | Where-Object { $_.Name -ne "README.md" } | ForEach-Object {
    $arch = Join-Path $archDir $_.Name
    if (-not (Test-Path $arch)) { $warns.Add("Feature '$($_.Name)' has no architecture doc yet.") }
  }
}

$warns  | ForEach-Object { Write-Host "[WARN] $_" -ForegroundColor Yellow }
if ($errors.Count -gt 0) {
  Write-Host "[FAIL] Architecture docs validation failed:" -ForegroundColor Red
  $errors | ForEach-Object { Write-Host "   - $_" -ForegroundColor Red }
  exit 1
}
Write-Host "[OK] Architecture documentation present and consistent." -ForegroundColor Green
exit 0
