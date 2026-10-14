<#
.SYNOPSIS
  Sobe os servicos do EnergyGuard via Docker Compose.
.PARAMETER Environment
  base | staging | production (padrao: base)
.PARAMETER Build
  Reconstroi a imagem (apenas util em 'base').
.EXAMPLE
  ./scripts/start.ps1 -Environment base -Build
  ./scripts/start.ps1 -Environment staging
#>
param(
    [ValidateSet('base','staging','production')][string]$Environment = 'base',
    [switch]$Build
)
. (Join-Path $PSScriptRoot '_common.ps1')

Assert-Docker
Assert-EnvFile
$files = Get-ComposeFiles -Environment $Environment

Write-Host "==> Subindo ambiente '$Environment'..." -ForegroundColor Cyan
$composeArgs = @('compose') + $files + @('up', '-d')
if ($Build -and $Environment -eq 'base') { $composeArgs += '--build' }

& docker @composeArgs
if ($LASTEXITCODE -ne 0) { throw "Falha ao subir os servicos (exit $LASTEXITCODE)." }

Write-Host "==> Servicos em execucao:" -ForegroundColor Green
& docker @(@('compose') + $files + @('ps'))
Write-Host "`nDica: verifique a saude com ./scripts/health.ps1 -Environment $Environment" -ForegroundColor Yellow
