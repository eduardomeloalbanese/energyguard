# Cidades ESG Inteligentes — EnergyGuard API

**Integrantes:** Eduardo Henrique Leite Rodrigues (RM564950) · Eduardo de Melo Albanese (RM561790)
**Disciplina:** Navegando pelo mundo DevOps

Solução DevOps para a API **EnergyGuard**, um sistema de monitoramento e gestão de
eficiência energética alinhado ao pilar **Ambiental (E)** do conceito **ESG** e à
visão de **Cidades Inteligentes**. Este repositório entrega a aplicação original
(ASP.NET Core 7) acrescida de containerização, orquestração, pipeline CI/CD,
testes automatizados, health checks e documentação técnica.

> **Transparência (leia primeiro):** o ambiente onde esta entrega foi preparada
> **não possui Docker nem servidores remotos**. Por isso, **build e testes foram
> executados de verdade** (evidências reais em `docs/evidencias/logs/`), enquanto
> **build da imagem Docker, execução de contêineres e deploy remoto em
> staging/produção estão como PENDENTES** — todos os arquivos e comandos estão
> prontos, mas dependem de uma máquina com Docker/infra. Veja
> [Evidências](#118-evidências) e [Problemas conhecidos](#119-problemas-conhecidos).

---

## 11.1. Identificação do projeto

- **Nome:** Cidades ESG Inteligentes — EnergyGuard.
- **Descrição:** API REST para cadastro de dispositivos de consumo, registro de
  leituras de energia (kWh), geração de alertas (limite excedido, dispositivo
  offline, anomalia) e relatórios de consumo com **pegada de carbono**
  (`CarbonFootprintKg`).
- **Problema que resolve:** cidades e organizações precisam **medir, monitorar e
  reduzir o consumo de energia e as emissões associadas**. O EnergyGuard
  centraliza leituras por dispositivo/local, sinaliza desperdícios e consolida
  indicadores que apoiam decisões de eficiência energética.
- **Principais funcionalidades:**
  - Autenticação JWT (registro/login) — `AuthController`.
  - CRUD de dispositivos — `DevicesController`.
  - Registro e consulta de leituras de energia — `EnergyReadingsController`.
  - Gestão de alertas e resolução — `AlertsController`.
  - Geração de relatórios de consumo e pegada de carbono — `ReportsController`.
  - Health checks de liveness e readiness.
  - Documentação interativa via Swagger.
- **Relação com práticas ESG:** o foco **Ambiental** aparece na medição de
  consumo (kWh), na identificação de desperdício (alertas de limite) e no cálculo
  da **pegada de carbono** dos relatórios — insumos diretos para metas de
  eficiência energética e descarbonização em ambientes urbanos inteligentes.
- **Tecnologias:** .NET 7 / ASP.NET Core 7, Entity Framework Core 7, SQL Server,
  ASP.NET Identity + JWT, FluentValidation, Swagger/Swashbuckle, xUnit, Docker,
  Docker Compose e GitHub Actions.

### Arquitetura (camadas)

```
EnergyGuard.Domain          -> Entidades e enums (regra de negócio pura)
EnergyGuard.Application      -> Serviços, interfaces, validações, ViewModels
EnergyGuard.Infrastructure   -> EF Core, DbContext, migrations, repositórios
EnergyGuard.API              -> Controllers, middleware, health checks, Program.cs
EnergyGuard.Tests            -> Testes de integração (xUnit + WebApplicationFactory)
```

> **Correção estrutural aplicada:** no projeto recebido, `EnergyGuard.Infrastructure`
> estava **dentro** de `EnergyGuard.Tests`, mas todos os `.csproj` o referenciam
> como projeto **irmão** (`..\EnergyGuard.Infrastructure\`). Nessa condição a
> solução **não compilava**. A infraestrutura foi movida para o nível correto e
> foi criado o arquivo de solução `EnergyGuard.sln`. Nenhuma regra de negócio,
> endpoint ou entidade foi alterada.

---

## 11.2. Pré-requisitos

| Ferramenta | Versão | Uso |
|------------|--------|-----|
| .NET SDK | **7.0.x** | build e testes locais |
| Docker | 24+ | build da imagem e execução |
| Docker Compose | **v2.24+** | orquestração (usa `!reset` nos overrides) |
| Git | 2.x | versionamento / CI |
| PowerShell | 5.1+ (Windows) | scripts de automação |

---

## 11.3. Como executar localmente com Docker

> Requer Docker + Docker Compose em execução.

1. **Obter o repositório** e entrar na pasta raiz (`EnergyGuard-Entrega/`).
2. **Configurar variáveis de ambiente** (copiar o exemplo e ajustar segredos):
   ```bash
   cp .env.example .env        # Linux/macOS
   ```
   ```powershell
   Copy-Item .env.example .env # Windows
   ```
   Ajuste `MSSQL_SA_PASSWORD` e `Jwt__Key` para valores fortes.
3. **Construir a imagem** (feito junto com o passo 4 via `--build`):
   ```bash
   docker compose build
   ```
4. **Iniciar os serviços**:
   ```bash
   docker compose up -d --build
   ```
   ou, no Windows: `./scripts/start.ps1 -Environment base -Build`
5. **Verificar os contêineres**:
   ```bash
   docker compose ps
   ```
6. **Testar a aplicação** (Swagger): http://localhost:8080/swagger
   - Liveness: http://localhost:8080/health/live
   - Readiness: http://localhost:8080/health/ready
7. **Consultar os logs**:
   ```bash
   docker compose logs -f api
   ```
   ou: `./scripts/logs.ps1 -Service api -Follow`
8. **Executar os testes** (local, sem Docker):
   ```bash
   dotnet test EnergyGuard.sln -c Release
   ```
   ou: `./scripts/test.ps1`
9. **Encerrar os serviços**:
   ```bash
   docker compose down
   ```
   ou: `./scripts/stop.ps1 -Environment base`
10. **Limpar recursos de demonstração** (⚠ apaga o volume do banco):
    ```bash
    docker compose down -v
    ```

---

## 11.4. Pipeline CI/CD

Arquivo: [`.github/workflows/ci-cd.yml`](.github/workflows/ci-cd.yml) — **GitHub Actions**.

- **Gatilhos:** `push` e `pull_request` nas branches `main`, `master`, `develop`;
  além de execução manual (`workflow_dispatch`).
- **Permissões mínimas:** `contents: read`, `packages: write` (para publicar no GHCR).
- **Jobs:**
  1. **build-test** — checkout → setup .NET 7 → `restore` → `build -c Release` →
     `test` com relatório `.trx` e cobertura (`XPlat Code Coverage`) → publica
     resultados (test-reporter) e artefatos. **Falha interrompe o pipeline.**
  2. **container-smoke-test** — **gera evidência real na nuvem do GitHub, sem Docker
     local nem servidor**: `docker compose up -d --build` (API + banco), aguarda o
     `/health/ready`, imprime `docker compose ps`, a imagem e os logs, e ainda
     demonstra os perfis de **staging (8081)** e **produção (8082)**. Os logs desse
     job são a prova de que a imagem builda, o contêiner sobe e fica saudável.
  3. **docker-build-push** (somente em `push`) — login no **GHCR**, geração de
     tags via `docker/metadata-action` e `build-push`. **Tags imutáveis por SHA**
     (`type=sha,format=long`) + tag por branch.
  4. **deploy-staging** — ambiente `staging`, via SSH + `docker compose` usando a
     imagem versionada por SHA, seguido de verificação de readiness (opcional,
     para servidor remoto real).
  5. **deploy-production** — ambiente `production` (**aprovação manual** via
     proteção de Environment), após staging, com verificação de readiness.
- **Segurança:** segredos via **GitHub Secrets**; nenhuma credencial embutida na
  imagem; produção não usa a tag `latest`.

> Os jobs de deploy só executam quando a **variável de repositório**
> `DEPLOY_ENABLED` for `true`. Sem infraestrutura configurada, eles são ignorados
> com segurança (o CI de build/testes continua funcionando normalmente).

### Pré-requisitos para ativar o deploy real

- Repositório no GitHub com Actions habilitado (GHCR já disponível por padrão).
- **Variáveis** (Settings → Secrets and variables → Actions → *Variables*):
  `DEPLOY_ENABLED=true`, `STAGING_URL`, `PRODUCTION_URL`.
- **Secrets**: `STAGING_SSH_HOST`, `STAGING_SSH_USER`, `STAGING_SSH_KEY`,
  `STAGING_DEPLOY_PATH` e os equivalentes `PRODUCTION_*`.
- **Environments** `staging` e `production` criados; em `production`, habilitar
  *Required reviewers* para aprovação manual.
- Servidores de staging/produção com Docker + Docker Compose e acesso ao GHCR.

---

## 11.5. Containerização

Arquivo: [`Dockerfile`](Dockerfile) (na raiz; `docker build` funciona a partir da raiz).

- **Build multi-stage**: `base` (runtime) → `build` (SDK, restore+publish) → `final`.
- **Imagens oficiais**: `mcr.microsoft.com/dotnet/aspnet:7.0` e `/sdk:7.0`.
- **Cache de camadas**: copia os `.csproj` e faz `restore` antes do código.
- **Usuário não-root**: a aplicação roda como `appuser` (UID 1001).
- **Porta**: `EXPOSE 8080` + `ASPNETCORE_URLS=http://+:8080` (o .NET 7 usaria 80 por padrão).
- **HEALTHCHECK**: `curl -f http://localhost:8080/health/live`.
- **`.dockerignore`**: evita copiar `bin/`, `obj/`, `.git/`, `.env`, docs e scripts.

---

## 11.6. Orquestração

Arquivo base: [`docker-compose.yml`](docker-compose.yml).

- **Serviços:** `api` (EnergyGuard) e `db` (SQL Server 2022).
- **Rede dedicada:** `energyguard-net` (bridge). A API acessa o banco pelo
  hostname interno `db`.
- **Volume persistente:** `mssql-data` → `/var/opt/mssql` (os dados sobrevivem a
  reinicializações dos contêineres).
- **Variáveis de ambiente:** connection string, JWT e porta vêm do `.env`.
- **Dependências:** `api` aguarda `db` ficar *healthy* (`depends_on: condition: service_healthy`).
- **Healthchecks:** `db` via `sqlcmd`; `api` via `curl` no `/health/live`.
- **Política de reinício:** `unless-stopped` (base/staging) e `always` (produção).
- **Segurança:** o banco **não** é exposto publicamente (porta comentada).
- **Schema/migrations:** aplicados automaticamente no startup da API
  (`Database.Migrate()` com retry; ignorado no provedor InMemory dos testes).

---

## 11.7. Ambientes (staging e produção)

- [`docker-compose.staging.yml`](docker-compose.staging.yml) — `ASPNETCORE_ENVIRONMENT=Staging`, porta host **8081**, volume próprio.
- [`docker-compose.production.yml`](docker-compose.production.yml) — `ASPNETCORE_ENVIRONMENT=Production`, porta host **8082**, `restart: always`, volume próprio.

Ambos **consomem a mesma imagem validada** (variável `IMAGE`, de preferência a
tag imutável por SHA do GHCR) em vez de reconstruir — garantindo consistência
entre ambientes.

**Demonstração local dos dois ambientes** (requer Docker):
```powershell
./scripts/validate-environments.ps1
```
Isso constrói a imagem local, sobe staging (8081) e produção (8082) e valida a
saúde de cada um.

> A execução local do perfil de produção é apenas **demonstração** — não
> representa um ambiente de produção público real. Para produção real, configure
> os servidores e os secrets descritos em [11.4](#114-pipeline-cicd).

**Recuperação em caso de falha (rollback):** como as imagens são versionadas por
SHA, o rollback consiste em redefinir `IMAGE` para o SHA anterior e repetir
`docker compose ... pull && up -d`.

---

## 11.8. Evidências

As evidências reais ficam em [`docs/evidencias/`](docs/evidencias/).

| Evidência | Status | Local / Como obter |
|-----------|--------|--------------------|
| Build da aplicação | ✅ **Real** | [`docs/evidencias/logs/01-build.log`](docs/evidencias/logs/01-build.log) — `dotnet build` com 0 erros |
| Testes aprovados | ✅ **Real** | [`docs/evidencias/logs/02-tests.log`](docs/evidencias/logs/02-tests.log) — 12/12 testes aprovados; [`docs/evidencias/test-results.trx`](docs/evidencias/test-results.trx) |
| Ambiente .NET | ✅ **Real** | [`docs/evidencias/logs/00-dotnet-info.log`](docs/evidencias/logs/00-dotnet-info.log) |
| Sintaxe dos YAML (workflow + 3 compose) | ✅ **Real** | [`docs/evidencias/logs/03-yaml-validation.log`](docs/evidencias/logs/03-yaml-validation.log) — validados com parser YAML real |
| Build da imagem Docker | ⏳ **PENDENTE** | **Via GitHub Actions** (job `container-smoke-test`/`docker-build-push`) ao fazer push — print do run verde. Ou local: `docker build -t energyguard-api .` |
| Execução dos contêineres + saúde | ⏳ **PENDENTE** | **Via GitHub Actions** (job `container-smoke-test`) — logs mostram `compose ps` e `/health/ready` OK. Ou local: `./scripts/capture-docker-evidence.ps1`. *Os endpoints já são validados nos testes (`HealthCheckTests`).* |
| Deploy em staging + produção | ⏳ **PENDENTE** | **Via GitHub Actions** (`container-smoke-test` demonstra os perfis staging/produção). Para servidor real: workflow com `DEPLOY_ENABLED=true` + secrets. Demo local: `./scripts/validate-environments.ps1` |

> Os itens pendentes **dependem de Docker e/ou de infraestrutura remota**, que não
> existiam no ambiente de preparação. Nenhum print, log ou resultado foi fabricado.
> Substitua os marcadores em `docs/evidencias/` pelas capturas reais ao reproduzir.
>
> **Atalho:** numa máquina com Docker, rode `./scripts/capture-docker-evidence.ps1`
> (ou com `-IncludeEnvironments`) — ele executa todo o fluxo, salva os logs reais em
> `docs/evidencias/logs/docker/` e indica exatamente quais telas printar.

### Gerar as evidências de Docker pelo GitHub (recomendado — sem Docker local)

O runner do GitHub Actions já tem Docker. Assim, **basta enviar o projeto ao GitHub**
para que o pipeline construa a imagem, suba os contêineres e valide a saúde — tudo na
nuvem, gerando evidência real para print.

1. Crie um repositório vazio no GitHub (ex.: `energyguard`).
2. Envie o conteúdo da pasta `EnergyGuard-Entrega/` para o repositório (via GitHub
   Desktop, `git push`, ou "Add file → Upload files" no site).
3. Abra a aba **Actions** → execução **CI/CD - EnergyGuard**.
4. Tire o print:
   - do run **verde** (jobs `build-test`, `container-smoke-test`, `docker-build-push`);
   - dos logs do job **container-smoke-test** mostrando `docker compose ps` e
     `/health/ready` OK, além de staging (8081) e produção (8082);
   - da imagem publicada em **Packages** (GHCR), com tag por SHA.
5. Salve os prints em `docs/evidencias/` substituindo os marcadores `PENDENTE-*.md`.

> Esse caminho é o que a atividade indica para comprovar a entrega e **não exige
> servidor próprio nem Docker na sua máquina**.

---

## 11.9. Problemas conhecidos

- **Docker ausente no ambiente de preparação:** build de imagem, execução de
  contêineres e healthcheck de container não foram executados aqui (apenas
  configurados). Build e testes .NET foram executados de verdade.
- **.NET 7 fora de suporte:** a versão foi **preservada** conforme o projeto
  original. Para longo prazo, recomenda-se migrar para .NET 8 LTS (ajustando as
  imagens base e os pacotes).
- **Segredos de desenvolvimento:** `appsettings.json` contém valores de JWT e
  connection string **apenas para desenvolvimento**. Em staging/produção eles são
  **sobrescritos** por variáveis de ambiente/secrets (precedência do ASP.NET Core).
- **Deploy remoto:** depende de servidores com Docker e dos secrets/variáveis do
  GitHub. Sem eles, os jobs de deploy são ignorados por design.
- **SQL Server em ARM/Apple Silicon:** pode exigir imagem/azure-sql-edge alternativa.

---

## 11.10. Checklist final

- [x] Projeto compactado em ZIP com estrutura organizada — `Entrega_DevOps_Cidades_ESG.zip`.
- [x] Dockerfile funcional (multi-stage, não-root, healthcheck) — *autoria validada; build da imagem pendente de Docker*.
- [x] Docker Compose (base + staging + produção, rede, volume, healthchecks) — *autoria validada; execução pendente de Docker*.
- [x] Pipeline com build, testes e deploy — build/testes prontos; deploy configurado e **gated** (pendente de infra/secrets).
- [x] README com instruções e evidências.
- [x] Documentação técnica em PDF — `docs/Documentacao_Tecnica_DevOps_ESG.pdf`.
- [ ] Deploy em staging e produção **comprovado** — **PENDENTE** (requer Docker/servidores). Arquivos e comandos prontos.

**Validado de verdade neste ambiente:** compilação (`dotnet build`, 0 erros) e
testes (`dotnet test`, **12/12 aprovados**). Demais itens de execução dependem de
Docker/infra e estão documentados como pendentes.

---

## Comandos rápidos

```bash
# Build + testes (sem Docker)
dotnet test EnergyGuard.sln -c Release

# Subir tudo localmente (com Docker)
docker compose up -d --build

# Saúde
curl http://localhost:8080/health/ready

# Derrubar
docker compose down
```

```powershell
# Equivalentes no Windows (PowerShell)
./scripts/test.ps1
./scripts/start.ps1 -Environment base -Build
./scripts/health.ps1 -Environment base
./scripts/stop.ps1 -Environment base
```
