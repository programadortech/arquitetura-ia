<#
.SYNOPSIS
  Busca issue(s) do GitHub e devolve JSON normalizado para a skill /import-story consumir.
.DESCRIPTION
  Tenta usar o `gh` CLI (se autenticado); caso contrário, usa a REST API do GitHub via
  Invoke-RestMethod com o token em $env:GITHUB_TOKEN. O repositório alvo é detectado a partir
  do remote `origin` (owner/repo), ou pode ser passado em -Repo.

  Modos:
    -Number <n>      busca um issue específico
    -List "<args>"   lista issues (repassa filtros, ex.: "--label historia --state open")

  Saída: JSON (um objeto, ou array no modo -List) com os campos:
    number, title, body, labels[], milestone, assignees[], url, state

  Segurança: o token NUNCA é impresso nem persistido. Use variável de ambiente / secret store.
.EXAMPLE
  pwsh scripts/import-github-issue.ps1 -Number 123
.EXAMPLE
  pwsh scripts/import-github-issue.ps1 -List "--label historia --state open"
#>
[CmdletBinding(DefaultParameterSetName = "Single")]
param(
  [Parameter(ParameterSetName = "Single", Mandatory = $true)] [int]$Number,
  [Parameter(ParameterSetName = "List",   Mandatory = $true)] [string]$List,
  [string]$Repo
)

$ErrorActionPreference = "Stop"

function Resolve-Repo {
  if ($Repo) { return $Repo }
  $url = (git config --get remote.origin.url) 2>$null
  if (-not $url) { throw "Não foi possível detectar o repositório: defina -Repo owner/name." }
  # suporta https e ssh
  if ($url -match 'github\.com[:/](?<owner>[^/]+)/(?<repo>[^/.]+)(\.git)?$') {
    return "$($Matches.owner)/$($Matches.repo)"
  }
  throw "Remote origin não parece ser um repositório GitHub: $url"
}

$repoSlug = Resolve-Repo
$hasGh = [bool](Get-Command gh -ErrorAction SilentlyContinue)

# ---- caminho gh CLI ----------------------------------------------------------
if ($hasGh) {
  $fields = "number,title,body,labels,milestone,assignees,url,state"
  if ($PSCmdlet.ParameterSetName -eq "Single") {
    & gh issue view $Number --repo $repoSlug --json $fields
  } else {
    # gh issue list aceita os filtros repassados em -List
    $listArgs = $List -split '\s+'
    & gh issue list --repo $repoSlug --json $fields @listArgs
  }
  exit $LASTEXITCODE
}

# ---- fallback REST API -------------------------------------------------------
if (-not $env:GITHUB_TOKEN) {
  Write-Error 'Nem o gh CLI esta instalado nem a variavel de ambiente GITHUB_TOKEN esta definida. Instale o gh (winget install GitHub.cli) ou defina um Personal Access Token em GITHUB_TOKEN.'
  exit 1
}

$headers = @{
  Authorization          = "Bearer $($env:GITHUB_TOKEN)"
  "Accept"               = "application/vnd.github+json"
  "X-GitHub-Api-Version" = "2022-11-28"
  "User-Agent"           = "arquitetura-ia-import"
}

function Convert-Issue($i) {
  [pscustomobject]@{
    number    = $i.number
    title     = $i.title
    body      = $i.body
    labels    = @($i.labels | ForEach-Object { $_.name })
    milestone = $(if ($i.milestone) { $i.milestone.title } else { $null })
    assignees = @($i.assignees | ForEach-Object { $_.login })
    url       = $i.html_url
    state     = $i.state
  }
}

if ($PSCmdlet.ParameterSetName -eq "Single") {
  $issue = Invoke-RestMethod -Headers $headers -Uri "https://api.github.com/repos/$repoSlug/issues/$Number"
  Convert-Issue $issue | ConvertTo-Json -Depth 6
} else {
  # tradução simples dos filtros mais comuns para query params da REST API
  $state = "open"; $labels = $null; $milestone = $null
  $tokens = $List -split '\s+'
  for ($k = 0; $k -lt $tokens.Count; $k++) {
    switch ($tokens[$k]) {
      "--state"     { $state = $tokens[$k+1]; $k++ }
      "--label"     { $labels = $tokens[$k+1]; $k++ }
      "--milestone" { $milestone = $tokens[$k+1]; $k++ }
    }
  }
  $q = "state=$state&per_page=100"
  if ($labels)    { $q += "&labels=$([uri]::EscapeDataString($labels))" }
  $issues = Invoke-RestMethod -Headers $headers -Uri "https://api.github.com/repos/$repoSlug/issues?$q"
  # REST inclui PRs em /issues; filtra apenas issues reais e, se pedido, por milestone
  $issues = $issues | Where-Object { -not $_.pull_request }
  if ($milestone) { $issues = $issues | Where-Object { $_.milestone -and $_.milestone.title -eq $milestone } }
  @($issues | ForEach-Object { Convert-Issue $_ }) | ConvertTo-Json -Depth 6
}
