<#
.SYNOPSIS
  Mostra os logs dos servicos.
.PARAMETER Environment
  base | staging | production (padrao: base)
.PARAMETER Service
  Servico especifico (ex.: api, db). Vazio = todos.
.PARAMETER Follow
  Acompanha os logs em tempo real.
.EXAMPLE
  ./scripts/logs.ps1 -Service api -Follow
#>
param(
    [ValidateSet('base','staging','production')][string]$Environment = 'base',
    [string]$Service = '',
    [switch]$Follow
)
. (Join-Path $PSScriptRoot '_common.ps1')

Assert-Docker
$files = Get-ComposeFiles -Environment $Environment
$composeArgs = @('compose') + $files + @('logs', '--tail', '200')
if ($Follow) { $composeArgs += '-f' }
if ($Service) { $composeArgs += $Service }

& docker @composeArgs
