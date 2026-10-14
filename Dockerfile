# ============================================================================
# EnergyGuard API - Dockerfile multi-stage (.NET 7)
# Build a partir da RAIZ da solucao:  docker build -t energyguard-api .
# ============================================================================

# ---- Estagio base (runtime) ----
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 8080
# .NET 7 escuta na porta 80 por padrao; fixamos 8080 explicitamente.
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# curl e usado pelo HEALTHCHECK (do Docker) e pelo healthcheck do compose.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Usuario nao-privilegiado para executar a aplicacao.
RUN adduser --disabled-password --gecos "" --uid 1001 appuser

# ---- Estagio de build ----
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copia apenas os .csproj primeiro para aproveitar o cache de camadas no restore.
COPY ["EnergyGuard.API/EnergyGuard.API.csproj", "EnergyGuard.API/"]
COPY ["EnergyGuard.Application/EnergyGuard.Application.csproj", "EnergyGuard.Application/"]
COPY ["EnergyGuard.Domain/EnergyGuard.Domain.csproj", "EnergyGuard.Domain/"]
COPY ["EnergyGuard.Infrastructure/EnergyGuard.Infrastructure.csproj", "EnergyGuard.Infrastructure/"]
RUN dotnet restore "EnergyGuard.API/EnergyGuard.API.csproj"

# Copia o restante do codigo-fonte (bin/obj ficam de fora via .dockerignore).
COPY . .
RUN dotnet publish "EnergyGuard.API/EnergyGuard.API.csproj" \
    -c Release -o /app/publish /p:UseAppHost=false

# ---- Estagio final (imagem enxuta de runtime) ----
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
USER appuser

HEALTHCHECK --interval=30s --timeout=5s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "EnergyGuard.API.dll"]
