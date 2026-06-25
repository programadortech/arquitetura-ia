<#
.SYNOPSIS
  Gera o documento OpenAPI de um produto em BUILD TIME (sem subir o servidor) e o publica em
  apps/<Produto>/contracts/openapi.json. Contrato versionado para o front consumir (ADR-0032).
.DESCRIPTION
  Usa o pacote Microsoft.Extensions.ApiDescription.Server (tool GetDocument), acionado via
  -p:OpenApiGenerateDocumentsOnBuild=true. A ferramenta constrói o host da API só para extrair o
  documento — por isso fornecemos valores dummy de Jwt:Key e ConnectionString (não abre banco, não
  vaza segredo). O JSON intermediário (obj/openapi) é copiado para o caminho canônico do contrato.
.PARAMETER Root
  Diretório do produto (default: apps/Plataforma2ASmart.Auth).
.EXAMPLE
  pwsh scripts/generate-openapi.ps1 -Root apps/Plataforma2ASmart.Auth
#>
[CmdletBinding()]
param([string]$Root = "apps/Plataforma2ASmart.Auth")

$ErrorActionPreference = "Stop"

$apiProj = Get-ChildItem -Path (Join-Path $Root "src") -Recurse -Filter "*.Api.csproj" -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $apiProj) { throw "Projeto *.Api.csproj não encontrado em $Root/src." }

# A tool constrói o host só para extrair o documento — sinaliza para o Program PULAR o seed (sem banco).
$env:OPENAPI_GENERATION = "true"
# Valores dummy só para a geração (registro de serviços; não conecta ao banco).
if (-not $env:Jwt__Key) { $env:Jwt__Key = "openapi-generation-dummy-key-0123456789abcdef" }
if (-not $env:ConnectionStrings__Default) { $env:ConnectionStrings__Default = "Server=(localdb)\\MSSQLLocalDB;Database=openapi-gen;Trusted_Connection=True;TrustServerCertificate=True" }

$outDir = Join-Path $apiProj.DirectoryName "obj/openapi"
if (Test-Path $outDir) { Remove-Item $outDir -Recurse -Force }

Write-Output "Gerando OpenAPI de $($apiProj.Name)..."
dotnet restore $apiProj.FullName | Out-Null
# --no-incremental garante que o target de geração do documento rode (não seja pulado por cache).
dotnet build $apiProj.FullName -c Debug --no-incremental -p:OpenApiGenerateDocumentsOnBuild=true | Out-Null
if ($LASTEXITCODE -ne 0) { throw "Build/geração do OpenAPI falhou." }

$generated = Get-ChildItem -Path $outDir -Filter "*.json" -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $generated) { throw "Documento OpenAPI não foi gerado em $outDir." }

$contractsDir = Join-Path $Root "contracts"
New-Item -ItemType Directory -Force -Path $contractsDir | Out-Null
$target = Join-Path $contractsDir "openapi.json"
Copy-Item $generated.FullName $target -Force

Write-Output "[OK] Contrato publicado: $target"
