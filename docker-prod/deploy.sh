#!/bin/bash

# Exit on error, undefined vars, and pipe failures
set -euo pipefail

# Get the directory where the script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# Project root is one level up from docker-prod/
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

DEPLOY_DIR="${SCRIPT_DIR}"
DEPLOY_BRANCH="${1:-main}"

echo "========================================="
echo " Email-Service — Manual Deployment"
echo " Branch  : ${DEPLOY_BRANCH}"
echo " Root    : ${PROJECT_ROOT}"
echo " $(date -u '+%Y-%m-%d %H:%M:%S UTC')"
echo "========================================="
echo ""
echo "NOTE: Global-Proxy must already be running ('web-network' must exist)."
echo ""

# ── Checkout the requested branch on VPS ──────────────────────────
echo ">>> Fetching and checking out '${DEPLOY_BRANCH}'..."
cd "${PROJECT_ROOT}"
git fetch origin
git checkout "${DEPLOY_BRANCH}"
git pull origin "${DEPLOY_BRANCH}"

cd "${DEPLOY_DIR}"

# ── Build and start services ──────────────────────────────────────
echo ">>> Building and starting services..."
docker compose build --no-cache
docker compose up -d --remove-orphans

echo ">>> Container status:"
docker compose ps

echo ""
echo ">>> Deployment complete!"
