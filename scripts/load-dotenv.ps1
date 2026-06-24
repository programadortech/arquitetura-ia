<#
.SYNOPSIS
  Carrega variáveis de um arquivo .env (na raiz do repo) para o ambiente do processo atual.
.DESCRIPTION
  Formato: linhas KEY=VALUE. Ignora linhas em branco e comentários (# ...). Aspas ao redor do valor
  são removidas. NÃO sobrescreve variáveis já definidas no ambiente (o ambiente real tem prioridade).
  Use para que scripts (import-tracker-issue, push-tracker-tasks) leiam segredos do .env sem 'setx'/restart.
  O .env é gitignored — segredos nunca vão para o git (ADR-0022).
.PARAMETER Path
  Caminho do arquivo .env. Padrão: .env na raiz do repositório (um nível acima de /scripts).
#>
[CmdletBinding()]
param([string]$Path)

if (-not $Path) {
  $Path = Join-Path (Split-Path $PSScriptRoot -Parent) ".env"
}
if (-not (Test-Path $Path)) {
  return  # sem .env: segue usando as variáveis de ambiente já existentes
}

foreach ($line in Get-Content -LiteralPath $Path) {
  $trimmed = $line.Trim()
  if (-not $trimmed -or $trimmed.StartsWith("#")) { continue }

  $eq = $trimmed.IndexOf("=")
  if ($eq -lt 1) { continue }

  $key = $trimmed.Substring(0, $eq).Trim()
  $val = $trimmed.Substring($eq + 1).Trim()

  # remove aspas simples/duplas ao redor do valor
  if ($val.Length -ge 2 -and (($val[0] -eq '"' -and $val[-1] -eq '"') -or ($val[0] -eq "'" -and $val[-1] -eq "'"))) {
    $val = $val.Substring(1, $val.Length - 2)
  }

  # não sobrescreve o que já está no ambiente (ambiente real > .env)
  if (-not [string]::IsNullOrEmpty([Environment]::GetEnvironmentVariable($key))) { continue }

  Set-Item -Path "Env:$key" -Value $val
}
