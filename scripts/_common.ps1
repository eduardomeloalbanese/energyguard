# Helpers compartilhados pelos scripts PowerShell do EnergyGuard.
$ErrorActionPreference = 'Stop'

function Get-RepoRoot {
    return (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
}

# Resolve o executavel 'dotnet' (PATH ou instalacao local do usuario).
function Resolve-Dotnet {
    $cmd = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    $local = Join-Path $env:USERPROFILE '.dotnet\dotnet.exe'
    if (Test-Path $local) { return $local }
    throw "dotnet nao encontrado. Instale o .NET SDK 7 ou adicione-o ao PATH."
}

# Resolve 'docker' e garante que esteja disponivel.
function Assert-Docker {
    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
        throw "docker nao encontrado. Instale o Docker Desktop e verifique se esta em execucao."
    }
}

# Mapeia o ambiente para a lista de arquivos compose (-f ...).
function Get-ComposeFiles {
    param([ValidateSet('base','staging','production')][string]$Environment = 'base')
    $root = Get-RepoRoot
    switch ($Environment) {
        'base'       { return @('-f', (Join-Path $root 'docker-compose.yml')) }
        'staging'    { return @('-f', (Join-Path $root 'docker-compose.yml'), '-f', (Join-Path $root 'docker-compose.staging.yml')) }
        'production' { return @('-f', (Join-Path $root 'docker-compose.yml'), '-f', (Join-Path $root 'docker-compose.production.yml')) }
    }
}

function Assert-EnvFile {
    $root = Get-RepoRoot
    $envFile = Join-Path $root '.env'
    if (-not (Test-Path $envFile)) {
        Write-Warning "Arquivo .env nao encontrado. Copiando de .env.example (ajuste os segredos antes de usar em producao)."
        Copy-Item (Join-Path $root '.env.example') $envFile
    }
}
