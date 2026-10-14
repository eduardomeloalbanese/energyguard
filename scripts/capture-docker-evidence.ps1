<#
.SYNOPSIS
  Captura AUTOMATICAMENTE todas as evidencias reais do Docker para a entrega.
  Rode este script em uma maquina COM Docker + Docker Compose em execucao.

  Ele executa o fluxo completo e salva os logs reais em:
      docs/evidencias/logs/docker/
  Ao final, mostra exatamente quais janelas/telas printar.

.PARAMETER IncludeEnvironments
  Tambem sobe e valida staging (8081) e producao (8082) como demonstracao local.

.EXAMPLE
  ./scripts/capture-docker-evidence.ps1
  ./scripts/capture-docker-evidence.ps1 -IncludeEnvironments
#>
param([switch]$IncludeEnvironments)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_common.ps1')

$root = Get-RepoRoot
$logDir = Join-Path $root 'docs\evidencias\logs\docker'
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

function Step($num, $title, $scriptblock) {
    $file = Join-Path $logDir ("{0}-{1}.log" -f $num, $title)
    Write-Host "`n==> [$num] $title" -ForegroundColor Cyan
    $stamp = (Get-Date).ToString('yyyy-MM-dd HH:mm:ss')
    "### $title  ($stamp)" | Out-File $file -Encoding utf8
    try {
        & $scriptblock *>&1 | Tee-Object -FilePath $file -Append
        Write-Host "    log: $file" -ForegroundColor DarkGray
    } catch {
        "ERRO: $($_.Exception.Message)" | Out-File $file -Append -Encoding utf8
        Write-Host "    FALHA (ver $file): $($_.Exception.Message)" -ForegroundColor Red
        throw
    }
}

# Pre-requisitos
Assert-Docker
Assert-EnvFile
Set-Location $root

Write-Host "Docker encontrado. Iniciando captura de evidencias..." -ForegroundColor Green

Step '10' 'docker-versions' { docker --version; docker compose version }
Step '11' 'docker-build'    { docker build -t energyguard-api:local . }
Step '12' 'compose-up'      { docker compose up -d --build }

# Aguarda os conteineres ficarem saudaveis
Write-Host "`n==> Aguardando readiness (ate ~90s)..." -ForegroundColor Cyan
$healthy = $false
for ($i = 1; $i -le 18; $i++) {
    Start-Sleep -Seconds 5
    try {
        $r = Invoke-WebRequest -Uri 'http://localhost:8080/health/ready' -UseBasicParsing -TimeoutSec 5
        if ($r.StatusCode -eq 200) { $healthy = $true; break }
    } catch { }
    Write-Host "    ...ainda subindo ($($i*5)s)" -ForegroundColor DarkGray
}

Step '13' 'compose-ps'      { docker compose ps }
Step '14' 'image-ls'        { docker image ls energyguard-api }
Step '15' 'health-live'     { (Invoke-WebRequest 'http://localhost:8080/health/live'  -UseBasicParsing).Content }
Step '16' 'health-ready'    { (Invoke-WebRequest 'http://localhost:8080/health/ready' -UseBasicParsing).Content }
Step '17' 'api-logs'        { docker compose logs --tail 80 api }

if (-not $healthy) {
    Write-Warning "A API nao respondeu 200 em /health/ready a tempo. Verifique 13-compose-ps.log e 17-api-logs.log."
}

if ($IncludeEnvironments) {
    $env:IMAGE = 'energyguard-api:local'
    Step '18' 'staging-up'    { docker compose -f docker-compose.yml -f docker-compose.staging.yml up -d }
    Start-Sleep -Seconds 12
    Step '19' 'staging-health'{ (Invoke-WebRequest 'http://localhost:8081/health/ready' -UseBasicParsing).Content }
    Step '20' 'production-up' { docker compose -f docker-compose.yml -f docker-compose.production.yml up -d }
    Start-Sleep -Seconds 12
    Step '21' 'production-health'{ (Invoke-WebRequest 'http://localhost:8082/health/ready' -UseBasicParsing).Content }
}

Write-Host "`n======================================================" -ForegroundColor Green
Write-Host " EVIDENCIAS CAPTURADAS em: $logDir" -ForegroundColor Green
Write-Host "======================================================" -ForegroundColor Green
Write-Host @"

PRINTS recomendados (tire o screenshot destas telas/janelas):
  1) Terminal mostrando '11-docker-build.log' (build concluido)       -> 03-docker-build.png
  2) Saida de 'docker compose ps' com STATUS 'healthy'                -> 04-containers-up.png
  3) Navegador em http://localhost:8080/health/ready mostrando Healthy -> 05-health.png
  4) (se usou -IncludeEnvironments) http://localhost:8081/health/ready -> 06-deploy-staging.png
  5) (se usou -IncludeEnvironments) http://localhost:8082/health/ready -> 07-deploy-production.png

Salve os PNGs em docs/evidencias/ (substituindo os marcadores PENDENTE-*.md).
Swagger para demonstrar a API: http://localhost:8080/swagger

Para encerrar tudo:
  ./scripts/stop.ps1 -Environment base
  ./scripts/stop.ps1 -Environment staging
  ./scripts/stop.ps1 -Environment production
"@ -ForegroundColor Yellow
