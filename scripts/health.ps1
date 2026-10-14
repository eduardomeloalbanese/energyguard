<#
.SYNOPSIS
  Verifica a saude da API (liveness e readiness).
.PARAMETER Environment
  base | staging | production (define a porta padrao: 8080 / 8081 / 8082)
.PARAMETER Port
  Sobrescreve a porta a ser consultada.
.EXAMPLE
  ./scripts/health.ps1 -Environment base
#>
param(
    [ValidateSet('base','staging','production')][string]$Environment = 'base',
    [int]$Port = 0
)
. (Join-Path $PSScriptRoot '_common.ps1')

if ($Port -eq 0) {
    $Port = switch ($Environment) { 'base' {8080} 'staging' {8081} 'production' {8082} }
}
$base = "http://localhost:$Port"
$ok = $true

foreach ($ep in @('/health/live', '/health/ready')) {
    $url = "$base$ep"
    try {
        $resp = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 10
        Write-Host ("[OK]   {0} -> {1} {2}" -f $url, [int]$resp.StatusCode, $resp.Content) -ForegroundColor Green
    }
    catch {
        $ok = $false
        Write-Host ("[FALHA] {0} -> {1}" -f $url, $_.Exception.Message) -ForegroundColor Red
    }
}

if (-not $ok) { exit 1 }
Write-Host "==> Aplicacao saudavel em $base" -ForegroundColor Green
