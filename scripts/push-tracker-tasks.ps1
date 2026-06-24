<#
.SYNOPSIS
  Cria as TASKS/atividades planejadas de uma história como itens-filho no tracker plugável
  (Azure DevOps, GitHub ou GitLab). Write-back do plano de implementação para o tracker.
.DESCRIPTION
  Lê o provider de .claude/tracker.config.json (ou -Provider). As tasks vêm de um arquivo JSON:
    [ { "title": "...", "description": "..." }, ... ]

  Comportamento por provider:
    - azure  : cria work items do tipo configurado (taskSync.azureTaskType, default "Task")
               com link de parentesco (Hierarchy-Reverse) para a história. Requer AZDO_PAT.
    - github : modo "checklist" — anexa uma seção "## Tasks" com itens - [ ] no corpo da issue.
               Requer gh autenticado ou GITHUB_TOKEN.
    - gitlab : modo "checklist" — anexa a seção de tasks à descrição da issue. Requer GITLAB_TOKEN.

  Segredos NUNCA no código — somente variáveis de ambiente.
.PARAMETER StoryId
  Id da história no tracker (work item id / issue number / iid).
.PARAMETER TasksFile
  Caminho para o JSON com as tasks.
.EXAMPLE
  pwsh scripts/push-tracker-tasks.ps1 -StoryId 12080 -TasksFile .tmp/tasks.json
#>
[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)] [string]$StoryId,
  [Parameter(Mandatory = $true)] [string]$TasksFile,
  [ValidateSet("github", "azure", "gitlab")] [string]$Provider,
  [string]$ConfigPath = ".claude/tracker.config.json"
)

$ErrorActionPreference = "Stop"

# ---- segredos: carrega o .env (gitignored) para o ambiente, se existir -------
& (Join-Path $PSScriptRoot "load-dotenv.ps1")

if (-not (Test-Path $TasksFile)) { throw "Arquivo de tasks não encontrado: $TasksFile" }
# Lê como UTF-8 explicitamente (PowerShell 5.1 usaria ANSI e corromperia acentos).
# Usa a forma de argumento do ConvertFrom-Json (o padrão "@(... | ConvertFrom-Json)" colapsa o array no PS 5.1).
$tasksRaw = [System.IO.File]::ReadAllText((Resolve-Path $TasksFile), [System.Text.Encoding]::UTF8)
$tasks = @(ConvertFrom-Json $tasksRaw)
if ($tasks.Count -eq 0) { Write-Output "[INFO] Nenhuma task para criar."; exit 0 }

$localCfg = ".claude/tracker.config.local.json"
if (-not $PSBoundParameters.ContainsKey('ConfigPath') -and (Test-Path $localCfg)) { $ConfigPath = $localCfg }
$cfg = $null
if (Test-Path $ConfigPath) { $cfg = Get-Content $ConfigPath -Raw | ConvertFrom-Json }
if (-not $Provider) {
  if ($cfg -and $cfg.provider) { $Provider = $cfg.provider }
  else { throw "Provider não definido. Use -Provider ou configure em $ConfigPath." }
}
function Get-Cfg([string]$s, [string]$k) { if ($cfg -and $cfg.$s -and $cfg.$s.$k) { return $cfg.$s.$k } return $null }

# ---- Azure DevOps ------------------------------------------------------------
function Push-Azure {
  if (-not $env:AZDO_PAT) { throw "azure: defina AZDO_PAT." }
  $org = Get-Cfg "azure" "org"; $project = Get-Cfg "azure" "project"
  if (-not $org -or -not $project) { throw "azure: configure org/project em $ConfigPath." }
  $taskType = (Get-Cfg "taskSync" "azureTaskType"); if (-not $taskType) { $taskType = "Task" }
  $auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$($env:AZDO_PAT)"))
  $orgEnc = [uri]::EscapeDataString($org); $projEnc = [uri]::EscapeDataString($project)
  $parentUrl = "https://dev.azure.com/$orgEnc/$projEnc/_apis/wit/workitems/$StoryId"

  $created = @()
  foreach ($t in $tasks) {
    $patch = @(
      @{ op = "add"; path = "/fields/System.Title"; value = $t.title }
    )
    if ($t.description) { $patch += @{ op = "add"; path = "/fields/System.Description"; value = $t.description } }
    $patch += @{ op = "add"; path = "/relations/-"; value = @{ rel = "System.LinkTypes.Hierarchy-Reverse"; url = $parentUrl } }
    $type = [uri]::EscapeDataString("`$$taskType")  # vira %24Task
    $uri = "https://dev.azure.com/$orgEnc/$projEnc/_apis/wit/workitems/$type`?api-version=7.1"
    $h = @{ Authorization = "Basic $auth" }
    # Envia o corpo em UTF-8 explicitamente (PowerShell 5.1 usa Latin-1 por padrão e corromperia acentos).
    $bodyBytes = [Text.Encoding]::UTF8.GetBytes(($patch | ConvertTo-Json -Depth 6))
    $r = Invoke-RestMethod -Method Post -Headers $h -Uri $uri -Body $bodyBytes -ContentType "application/json-patch+json; charset=utf-8"
    $created += [pscustomobject]@{ id = $r.id; title = $t.title }
    Write-Output "  [OK] Task #$($r.id): $($t.title)"
  }
  Write-Output "[OK] $($created.Count) task(s) criada(s) como filhas da história #$StoryId no Azure DevOps."
}

# ---- GitHub (checklist) ------------------------------------------------------
function Push-GitHub {
  $repo = Get-Cfg "github" "repo"
  if (-not $repo) {
    $url = (git config --get remote.origin.url) 2>$null
    if ($url -match 'github\.com[:/](?<o>[^/]+)/(?<r>[^/.]+)(\.git)?$') { $repo = "$($Matches.o)/$($Matches.r)" }
  }
  if (-not $repo) { throw "github: configure github.repo ou tenha remote origin." }
  $section = "`n`n## Tasks (planejadas pela IA)`n" + (($tasks | ForEach-Object { "- [ ] $($_.title)" }) -join "`n")

  if (Get-Command gh -ErrorAction SilentlyContinue) {
    $cur = & gh issue view $StoryId --repo $repo --json body | ConvertFrom-Json
    $new = ($cur.body) + $section
    $tmp = New-TemporaryFile; Set-Content -Path $tmp -Value $new -Encoding utf8
    & gh issue edit $StoryId --repo $repo --body-file $tmp | Out-Null
    Remove-Item $tmp -Force
  } else {
    if (-not $env:GITHUB_TOKEN) { throw "github: instale gh ou defina GITHUB_TOKEN." }
    $h = @{ Authorization = "Bearer $($env:GITHUB_TOKEN)"; Accept = "application/vnd.github+json"; "User-Agent" = "arquitetura-ia" }
    $cur = Invoke-RestMethod -Headers $h -Uri "https://api.github.com/repos/$repo/issues/$StoryId"
    $new = ($cur.body) + $section
    Invoke-RestMethod -Method Patch -Headers $h -Uri "https://api.github.com/repos/$repo/issues/$StoryId" -Body (@{ body = $new } | ConvertTo-Json) | Out-Null
  }
  Write-Output "[OK] $($tasks.Count) task(s) anexada(s) como checklist na issue #$StoryId (GitHub)."
}

# ---- GitLab (checklist) ------------------------------------------------------
function Push-GitLab {
  if (-not $env:GITLAB_TOKEN) { throw "gitlab: defina GITLAB_TOKEN." }
  $projectId = Get-Cfg "gitlab" "projectId"; $baseUrl = Get-Cfg "gitlab" "host"
  if (-not $baseUrl) { $baseUrl = "https://gitlab.com" }
  if (-not $projectId) { throw "gitlab: configure projectId em $ConfigPath." }
  $h = @{ "PRIVATE-TOKEN" = $env:GITLAB_TOKEN }
  $cur = Invoke-RestMethod -Headers $h -Uri "$baseUrl/api/v4/projects/$projectId/issues/$StoryId"
  $section = "`n`n## Tasks (planejadas pela IA)`n" + (($tasks | ForEach-Object { "- [ ] $($_.title)" }) -join "`n")
  $new = ($cur.description) + $section
  Invoke-RestMethod -Method Put -Headers $h -Uri "$baseUrl/api/v4/projects/$projectId/issues/$StoryId" -Body @{ description = $new } | Out-Null
  Write-Output "[OK] $($tasks.Count) task(s) anexada(s) como checklist na issue #$StoryId (GitLab)."
}

switch ($Provider) {
  "azure"  { Push-Azure }
  "github" { Push-GitHub }
  "gitlab" { Push-GitLab }
}
