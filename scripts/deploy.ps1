<#
.SYNOPSIS
  Promove (deploy) uma imagem versionada para staging ou producao via Docker Compose.
  Usa a MESMA imagem (variavel IMAGE) nos dois ambientes para garantir consistencia.
.PARAMETER Environment
  staging | production
.PARAMETER Image
  Referencia completa da imagem com tag imutavel (ex.: ghcr.io/owner/repo:sha-<...>).
  Se omitido, usa a variavel de ambiente IMAGE ou a definida no .env.
.EXAMPLE
  ./scripts/deploy.ps1 -Environment staging -Image ghcr.io/owner/energyguard:sha-abc123
#>
param(
    [Parameter(Mandatory)][ValidateSet('staging','production')][string]$Environment,
    [string]$Image = $env:IMAGE
)
. (Join-Path $PSScriptRoot '_common.ps1')

Assert-Docker
Assert-EnvFile
if ($Image) { $env:IMAGE = $Image }
if (-not $env:IMAGE) {
    throw "Defina a imagem: parametro -Image ou variavel IMAGE (ex.: ghcr.io/owner/repo:sha-<...>)."
}

$files = Get-ComposeFiles -Environment $Environment
Write-Host "==> Deploy em '$Environment' com imagem: $($env:IMAGE)" -ForegroundColor Cyan

& docker @(@('compose') + $files + @('pull'))
if ($LASTEXITCODE -ne 0) { throw "Falha no pull da imagem." }

& docker @(@('compose') + $files + @('up', '-d'))
if ($LASTEXITCODE -ne 0) { throw "Falha ao atualizar os servicos." }

Write-Host "==> Aguardando readiness..." -ForegroundColor Cyan
Start-Sleep -Seconds 8
& (Join-Path $PSScriptRoot 'health.ps1') -Environment $Environment
if ($LASTEXITCODE -ne 0) {
    Write-Warning "Readiness falhou. Considere rollback: docker compose <files> rollback / voltar a imagem anterior."
    exit 1
}
Write-Host "==> Deploy em '$Environment' concluido." -ForegroundColor Green
