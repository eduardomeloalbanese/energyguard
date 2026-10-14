<#
.SYNOPSIS
  Demonstracao local: sobe staging (porta 8081) e producao (porta 8082)
  usando a MESMA imagem, e valida a saude dos dois ambientes.
  ATENCAO: isto e apenas uma demonstracao local, nao representa producao publica.
.PARAMETER Image
  Imagem a utilizar (padrao: energyguard-api:local, construida localmente).
.EXAMPLE
  ./scripts/validate-environments.ps1
#>
param(
    [string]$Image = 'energyguard-api:local'
)
. (Join-Path $PSScriptRoot '_common.ps1')

Assert-Docker
Assert-EnvFile
$root = Get-RepoRoot

Write-Host "==> Construindo a imagem local '$Image'..." -ForegroundColor Cyan
& docker build -t $Image $root
if ($LASTEXITCODE -ne 0) { throw "Falha no build da imagem." }
$env:IMAGE = $Image

try {
    Write-Host "`n===== STAGING (porta 8081) =====" -ForegroundColor Magenta
    $staging = Get-ComposeFiles -Environment staging
    & docker @(@('compose') + $staging + @('up', '-d'))
    Start-Sleep -Seconds 10
    & (Join-Path $PSScriptRoot 'health.ps1') -Environment staging
    $stagingOk = $LASTEXITCODE -eq 0

    Write-Host "`n===== PRODUCTION (porta 8082) =====" -ForegroundColor Magenta
    $prod = Get-ComposeFiles -Environment production
    & docker @(@('compose') + $prod + @('up', '-d'))
    Start-Sleep -Seconds 10
    & (Join-Path $PSScriptRoot 'health.ps1') -Environment production
    $prodOk = $LASTEXITCODE -eq 0

    Write-Host "`n==> Resultado: staging=$stagingOk producao=$prodOk" -ForegroundColor Cyan
    if (-not ($stagingOk -and $prodOk)) { exit 1 }
}
finally {
    Write-Host "`nPara encerrar: ./scripts/stop.ps1 -Environment staging ; ./scripts/stop.ps1 -Environment production" -ForegroundColor Yellow
}
