<#
.SYNOPSIS
  Validates relational DB migration scripts under db/<provider>/migrations for naming, ordering,
  reversibility and safety. Provider-agnostic: oracle | sqlserver | postgresql | mysql.
.DESCRIPTION
  Checks each project's db/<provider>/migrations:
    1. File names match V<NNNN>__<desc>.sql (up) / U<NNNN>__<desc>.sql (down).
    2. Version numbers are sequential without gaps.
    3. Each up script has a down script OR a header comment explaining why not.
    4. Destructive DDL (DROP/TRUNCATE/DROP COLUMN) is flagged unless a guard comment is present.
  Exit 0 = pass, 1 = violations.
#>
[CmdletBinding()]
param([string]$Root = ".")

$ErrorActionPreference = "Stop"
$violations = New-Object System.Collections.Generic.List[string]
$warns      = New-Object System.Collections.Generic.List[string]

$migDirs = Get-ChildItem -Path $Root -Recurse -Directory -ErrorAction SilentlyContinue |
           Where-Object { $_.FullName -match '[\\/]db[\\/](oracle|sqlserver|postgresql|mysql)[\\/]migrations$' }

if (-not $migDirs) {
  Write-Host "[INFO] no db/<provider>/migrations directory found - nothing to validate." -ForegroundColor Yellow
  exit 0
}

foreach ($dir in $migDirs) {
  $files = Get-ChildItem $dir.FullName -Filter "*.sql" | Sort-Object Name
  $ups   = $files | Where-Object { $_.Name -match '^V\d{4}__.+\.sql$' }
  $downs = $files | Where-Object { $_.Name -match '^U\d{4}__.+\.sql$' }

  foreach ($f in $files) {
    if ($f.Name -notmatch '^[VU]\d{4}__.+\.sql$') {
      $violations.Add("Bad script name (expected V/U + 4 digits + '__' + desc): $($f.FullName)")
    }
  }

  # sequential up versions
  $expected = 1
  foreach ($u in ($ups | Sort-Object Name)) {
    $num = [int]($u.Name.Substring(1,4))
    if ($num -ne $expected) { $violations.Add("Migration version gap in $($dir.FullName): expected V$($expected.ToString('0000')), found $($u.Name)") }
    $expected++
  }

  # down coverage + destructive guard
  foreach ($u in $ups) {
    $ver  = $u.Name.Substring(1,4)
    $down = $downs | Where-Object { $_.Name.Substring(1,4) -eq $ver }
    $body = Get-Content $u.FullName -Raw
    if (-not $down -and $body -notmatch '(?im)^\s*--\s*no[- ]rollback') {
      $warns.Add("No down script for V$ver and no '-- no-rollback: <reason>' header: $($u.Name)")
    }
    if ($body -match '(?im)\b(DROP\s+(TABLE|COLUMN|USER|INDEX)|TRUNCATE\s+TABLE)\b' -and
        $body -notmatch '(?im)^\s*--\s*destructive-ok') {
      $violations.Add("Destructive DDL without '-- destructive-ok: <reason>' guard: $($u.FullName)")
    }
  }
}

$warns | ForEach-Object { Write-Host "[WARN] $_" -ForegroundColor Yellow }
if ($violations.Count -gt 0) {
  Write-Host "[FAIL] DB script validation failed:" -ForegroundColor Red
  $violations | ForEach-Object { Write-Host "   - $_" -ForegroundColor Red }
  exit 1
}
Write-Host "[OK] DB scripts valid." -ForegroundColor Green
exit 0
