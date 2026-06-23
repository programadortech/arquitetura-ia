<#
.SYNOPSIS
  Busca história(s)/work item(s) de um tracker plugável (GitHub, Azure DevOps ou GitLab) e devolve
  JSON NORMALIZADO para a skill /import-story consumir — independente do provider.
.DESCRIPTION
  O provider é escolhido por configuração (.claude/tracker.config.json) ou pelo parâmetro -Provider.
  Cada provider tem um adapter, mas todos retornam o MESMO contrato normalizado (a "porta"):

    { provider, key, number, title, body, acceptanceCriteriaRaw,
      labels[], milestone, assignees[], url, state }

  Autenticação (segredos NUNCA no código — somente variáveis de ambiente):
    - github : gh CLI autenticado OU $env:GITHUB_TOKEN (REST api.github.com)
    - azure  : $env:AZDO_PAT                          (REST dev.azure.com, Basic auth)
    - gitlab : $env:GITLAB_TOKEN                       (REST <host>/api/v4, header PRIVATE-TOKEN)

  Config (.claude/tracker.config.json), exemplo:
    {
      "provider": "github",
      "github": { "repo": "" },                       // vazio = detecta do remote origin
      "azure":  { "org": "minhaorg", "project": "MeuProjeto" },
      "gitlab": { "host": "https://gitlab.com", "projectId": "123" }
    }
.PARAMETER Id
  Identificador do item no tracker (número da issue/iid/work item id).
.PARAMETER Provider
  Sobrescreve o provider do config: github | azure | gitlab.
.EXAMPLE
  pwsh scripts/import-tracker-issue.ps1 -Id 42
.EXAMPLE
  pwsh scripts/import-tracker-issue.ps1 -Provider azure -Id 1234
#>
[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)] [string]$Id,
  [ValidateSet("github", "azure", "gitlab")] [string]$Provider,
  [string]$ConfigPath = ".claude/tracker.config.json"
)

$ErrorActionPreference = "Stop"

# ---- configuração ------------------------------------------------------------
$cfg = $null
if (Test-Path $ConfigPath) { $cfg = Get-Content $ConfigPath -Raw | ConvertFrom-Json }
if (-not $Provider) {
  if ($cfg -and $cfg.provider) { $Provider = $cfg.provider }
  else { throw "Provider não definido. Use -Provider ou configure 'provider' em $ConfigPath." }
}

function Get-Cfg([string]$section, [string]$key) {
  if ($cfg -and $cfg.$section -and $cfg.$section.$key) { return $cfg.$section.$key }
  return $null
}

function Resolve-GitHubRepo {
  $r = Get-Cfg "github" "repo"
  if ($r) { return $r }
  $url = (git config --get remote.origin.url) 2>$null
  if ($url -match 'github\.com[:/](?<owner>[^/]+)/(?<repo>[^/.]+)(\.git)?$') {
    return "$($Matches.owner)/$($Matches.repo)"
  }
  throw "github.repo não configurado e não foi possível detectar pelo remote origin."
}

# ---- adapters ----------------------------------------------------------------
function Get-GitHubIssue([string]$repo, [string]$num) {
  if (Get-Command gh -ErrorAction SilentlyContinue) {
    $j = & gh issue view $num --repo $repo --json number,title,body,labels,milestone,assignees,url,state | ConvertFrom-Json
    return [pscustomobject]@{
      provider = "github"; key = "GH-$($j.number)"; number = $j.number
      title = $j.title; body = $j.body; acceptanceCriteriaRaw = $null
      labels = @($j.labels | ForEach-Object { $_.name })
      milestone = $(if ($j.milestone) { $j.milestone.title } else { $null })
      assignees = @($j.assignees | ForEach-Object { $_.login })
      url = $j.url; state = $j.state
    }
  }
  if (-not $env:GITHUB_TOKEN) { throw "github: instale o gh CLI ou defina GITHUB_TOKEN." }
  $h = @{ Authorization = "Bearer $($env:GITHUB_TOKEN)"; Accept = "application/vnd.github+json"; "X-GitHub-Api-Version" = "2022-11-28"; "User-Agent" = "arquitetura-ia" }
  $i = Invoke-RestMethod -Headers $h -Uri "https://api.github.com/repos/$repo/issues/$num"
  [pscustomobject]@{
    provider = "github"; key = "GH-$($i.number)"; number = $i.number
    title = $i.title; body = $i.body; acceptanceCriteriaRaw = $null
    labels = @($i.labels | ForEach-Object { $_.name })
    milestone = $(if ($i.milestone) { $i.milestone.title } else { $null })
    assignees = @($i.assignees | ForEach-Object { $_.login })
    url = $i.html_url; state = $i.state
  }
}

function Get-AzureWorkItem([string]$org, [string]$project, [string]$wid) {
  if (-not $env:AZDO_PAT) { throw "azure: defina AZDO_PAT (Personal Access Token, escopo Work Items Read)." }
  if (-not $org -or -not $project) { throw "azure: configure 'org' e 'project' em $ConfigPath." }
  $auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$($env:AZDO_PAT)"))
  $h = @{ Authorization = "Basic $auth" }
  $orgEnc = [uri]::EscapeDataString($org)
  $projEnc = [uri]::EscapeDataString($project)
  $uri = "https://dev.azure.com/$orgEnc/$projEnc/_apis/wit/workitems/$wid`?api-version=7.1"
  $w = Invoke-RestMethod -Headers $h -Uri $uri
  $f = $w.fields
  [pscustomobject]@{
    provider = "azure"; key = "AZ-$($w.id)"; number = $w.id
    title = $f.'System.Title'; body = $f.'System.Description'
    acceptanceCriteriaRaw = $f.'Microsoft.VSTS.Common.AcceptanceCriteria'   # HTML
    labels = @($(if ($f.'System.Tags') { ($f.'System.Tags' -split ';').Trim() } else { @() }))
    milestone = $f.'System.IterationPath'
    assignees = @($(if ($f.'System.AssignedTo') { $f.'System.AssignedTo'.displayName } else { @() }))
    url = "https://dev.azure.com/$orgEnc/$projEnc/_workitems/edit/$($w.id)"; state = $f.'System.State'
  }
}

function Get-GitLabIssue([string]$baseUrl, [string]$projectId, [string]$iid) {
  if (-not $env:GITLAB_TOKEN) { throw "gitlab: defina GITLAB_TOKEN (escopo read_api)." }
  if (-not $projectId) { throw "gitlab: configure 'projectId' em $ConfigPath." }
  if (-not $baseUrl) { $baseUrl = "https://gitlab.com" }
  $h = @{ "PRIVATE-TOKEN" = $env:GITLAB_TOKEN }
  $i = Invoke-RestMethod -Headers $h -Uri "$baseUrl/api/v4/projects/$projectId/issues/$iid"
  [pscustomobject]@{
    provider = "gitlab"; key = "GL-$($i.iid)"; number = $i.iid
    title = $i.title; body = $i.description; acceptanceCriteriaRaw = $null
    labels = @($i.labels)
    milestone = $(if ($i.milestone) { $i.milestone.title } else { $null })
    assignees = @($i.assignees | ForEach-Object { $_.username })
    url = $i.web_url; state = $i.state
  }
}

# ---- dispatch ----------------------------------------------------------------
switch ($Provider) {
  "github" { $result = Get-GitHubIssue (Resolve-GitHubRepo) $Id }
  "azure"  { $result = Get-AzureWorkItem (Get-Cfg "azure" "org") (Get-Cfg "azure" "project") $Id }
  "gitlab" { $result = Get-GitLabIssue (Get-Cfg "gitlab" "host") (Get-Cfg "gitlab" "projectId") $Id }
}

$result | ConvertTo-Json -Depth 6
