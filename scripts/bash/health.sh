#!/usr/bin/env bash
# Verifica a saude da API EnergyGuard.
# Uso: ./health.sh [URL_BASE]   (padrao: http://localhost:8080)
set -euo pipefail

BASE="${1:-http://localhost:8080}"
STATUS=0

for ep in /health/live /health/ready; do
  if curl -fsS "${BASE}${ep}" >/dev/null; then
    echo "[OK]    ${BASE}${ep}"
  else
    echo "[FALHA] ${BASE}${ep}" >&2
    STATUS=1
  fi
done

exit $STATUS
