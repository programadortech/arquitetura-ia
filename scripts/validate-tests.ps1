<#
.SYNOPSIS
  Validates the test suite: the three test projects exist and `dotnet test` passes.
.DESCRIPTION
  Checks:
    1. UnitTests, IntegrationTests and ArchitectureTests projects exist (per solution).
    2. Runs `dotnet test` if a solution and the dotnet SDK are present.
  Use -SkipRun to validate structure only (fast pre-commit). Exit 0 = pass, 1 = fail.
#>
[CmdletBinding()]
param([string]$Root = ".", [switch]$SkipRun)

$ErrorActionPreference = "Stop"
$errors = New-Object System.Collections.Generic.List[string]

$sln = Get-ChildItem -Path $Root -Recurse -Filter *.sln -ErrorAction SilentlyContinue |
       Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } | Select-Object -First 1
if (-not $sln) {
  Write-Host "[INFO] no .sln found - nothing to test yet." -ForegroundColor Yellow
  exit 0
}

$testProjects = Get-ChildItem -Path $Root -Recurse -Filter *.csproj -ErrorAction SilentlyContinue |
                Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' }
foreach ($suffix in @("UnitTests","IntegrationTests","ArchitectureTests")) {
  if (-not ($testProjects | Where-Object { $_.BaseName -match "\.$suffix$" })) {
    $errors.Add("Missing test project: *.$suffix")
  }
}

if ($errors.Count -gt 0) {
  Write-Host "[FAIL] Test structure validation failed:" -ForegroundColor Red
  $errors | ForEach-Object { Write-Host "   - $_" -ForegroundColor Red }
  exit 1
}
Write-Host "[OK] Required test projects present." -ForegroundColor Green

if (-not $SkipRun -and (Get-Command dotnet -ErrorAction SilentlyContinue)) {
  Write-Host "> dotnet test $($sln.Name)..."
  & dotnet test $sln.FullName --nologo -v minimal
  if ($LASTEXITCODE -ne 0) { Write-Host "[FAIL] dotnet test failed." -ForegroundColor Red; exit 1 }
  Write-Host "[OK] dotnet test passed." -ForegroundColor Green
} else {
  Write-Host "[INFO] skipped test run (SkipRun set or dotnet not found)." -ForegroundColor Yellow
}
exit 0
