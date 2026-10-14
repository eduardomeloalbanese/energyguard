#!/usr/bin/env bash
# Deploy do EnergyGuard em staging/producao via Docker Compose (Linux/CI/servidor).
# Uso: ./deploy.sh <staging|production> <IMAGE>
#   IMAGE ex.: ghcr.io/owner/repo:sha-<...>  (tag imutavel)
set -euo pipefail

ENVIRONMENT="${1:-}"
export IMAGE="${2:-${IMAGE:-}}"

if [[ "$ENVIRONMENT" != "staging" && "$ENVIRONMENT" != "production" ]]; then
  echo "Uso: $0 <staging|production> <IMAGE>" >&2
  exit 1
fi
if [[ -z "${IMAGE}" ]]; then
  echo "Defina a IMAGE (2o argumento ou variavel de ambiente IMAGE)." >&2
  exit 1
fi

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT"
FILES=(-f docker-compose.yml -f "docker-compose.${ENVIRONMENT}.yml")

echo "==> Deploy em ${ENVIRONMENT} com imagem ${IMAGE}"
docker compose "${FILES[@]}" pull
docker compose "${FILES[@]}" up -d

# Readiness
case "$ENVIRONMENT" in
  staging) PORT=8081;;
  production) PORT=8082;;
esac
echo "==> Verificando readiness em http://localhost:${PORT}/health/ready ..."
for i in $(seq 1 10); do
  if curl -fsS "http://localhost:${PORT}/health/ready" >/dev/null; then
    echo "==> ${ENVIRONMENT} saudavel."
    exit 0
  fi
  sleep 6
done
echo "==> ${ENVIRONMENT} nao ficou saudavel a tempo." >&2
exit 1
