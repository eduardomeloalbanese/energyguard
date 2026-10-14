<#
.SYNOPSIS
  Executa build e testes automatizados (xUnit) localmente, sem Docker.
.EXAMPLE
  ./scripts/test.ps1
#>
. (Join-Path $PSScriptRoot '_common.ps1')

$dotnet = Resolve-Dotnet
$root = Get-RepoRoot
$sln = Join-Path $root 'EnergyGuard.sln'
$results = Join-Path $root 'TestResults'

Write-Host "==> Restaurando dependencias..." -ForegroundColor Cyan
& $dotnet restore $sln
if ($LASTEXITCODE -ne 0) { throw "Falha no restore." }

Write-Host "==> Compilando (Release)..." -ForegroundColor Cyan
& $dotnet build $sln -c Release --no-restore
if ($LASTEXITCODE -ne 0) { throw "Falha no build." }

Write-Host "==> Executando testes (Release)..." -ForegroundColor Cyan
& $dotnet test $sln -c Release --no-build `
    --logger "trx;LogFileName=test-results.trx" `
    --collect:"XPlat Code Coverage" `
    --results-directory $results
if ($LASTEXITCODE -ne 0) { throw "Testes falharam (exit $LASTEXITCODE)." }

Write-Host "==> Testes aprovados. Resultados em: $results" -ForegroundColor Green
