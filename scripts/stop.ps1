<#
.SYNOPSIS
  Encerra os servicos do EnergyGuard.
.PARAMETER Environment
  base | staging | production (padrao: base)
.PARAMETER Volumes
  Tambem remove os volumes (APAGA os dados persistidos do banco!).
.EXAMPLE
  ./scripts/stop.ps1 -Environment base
  ./scripts/stop.ps1 -Environment base -Volumes   # cuidado: apaga dados
#>
param(
    [ValidateSet('base','staging','production')][string]$Environment = 'base',
    [switch]$Volumes
)
. (Join-Path $PSScriptRoot '_common.ps1')

Assert-Docker
$files = Get-ComposeFiles -Environment $Environment
$composeArgs = @('compose') + $files + @('down')
if ($Volumes) {
    Write-Warning "A flag -Volumes ira APAGAR os dados persistidos do banco deste ambiente."
    $composeArgs += '-v'
}

Write-Host "==> Encerrando ambiente '$Environment'..." -ForegroundColor Cyan
& docker @composeArgs
if ($LASTEXITCODE -ne 0) { throw "Falha ao encerrar os servicos (exit $LASTEXITCODE)." }
Write-Host "==> Ambiente '$Environment' encerrado." -ForegroundColor Green
