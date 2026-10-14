# Evidências de execução

Esta pasta reúne as evidências **reais** da entrega. Nada aqui é fabricado.

## ✅ Evidências reais já disponíveis

- [`logs/00-dotnet-info.log`](logs/00-dotnet-info.log) — ambiente .NET utilizado.
- [`logs/01-build.log`](logs/01-build.log) — saída real de `dotnet build -c Release`
  (**0 erros, 0 avisos**).
- [`logs/02-tests.log`](logs/02-tests.log) — saída real de `dotnet test`
  (**12 testes, 12 aprovados, 0 falhas**).
- [`test-results.trx`](test-results.trx) — relatório de testes gerado pela execução.

## ⚡ Captura automática (recomendado)

Em uma máquina **com Docker + Docker Compose**, rode um único comando que executa
todo o fluxo e salva os logs reais em `logs/docker/`, indicando o que printar:

```powershell
./scripts/capture-docker-evidence.ps1                   # api + banco (porta 8080)
./scripts/capture-docker-evidence.ps1 -IncludeEnvironments  # + staging(8081) e produção(8082)
```

## ⏳ Evidências pendentes (requerem Docker / infraestrutura)

Os arquivos `PENDENTE-*.md` abaixo são **marcadores**. Ao reproduzir em uma
máquina com Docker (e/ou servidores), capture o print e salve com o mesmo nome
(ex.: `03-docker-build.png`), substituindo o marcador.

| Marcador | Como obter a evidência |
|----------|------------------------|
| `PENDENTE-03-docker-build.md` | `docker build -t energyguard-api .` → print do sucesso |
| `PENDENTE-04-containers-up.md` | `docker compose up -d` e `docker compose ps` → print |
| `PENDENTE-05-health.md` | `curl http://localhost:8080/health/ready` → print |
| `PENDENTE-06-deploy-staging.md` | `./scripts/validate-environments.ps1` ou workflow com `DEPLOY_ENABLED=true` → print |
| `PENDENTE-07-deploy-production.md` | Deploy em produção com aprovação manual → print |

> **Observação:** os endpoints de saúde (`/health/live`, `/health/ready`, `/health`)
> já são exercitados de verdade pelos testes de integração (`HealthCheckTests`),
> cujo resultado consta em `logs/02-tests.log`.
