#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT_PATH="$ROOT_DIR/src/frontend/NewDevicesLab.Frontend.csproj"
OUT_DIR="$ROOT_DIR/publish/monsterasp"

echo "Publishing NewDevicesLab frontend for MonsterASP..."
rm -rf "$OUT_DIR"

DOTNET_ENVIRONMENT=Production \
ASPNETCORE_ENVIRONMENT=Production \
dotnet publish "$PROJECT_PATH" -c Release -o "$OUT_DIR"

echo "Publish completed: $OUT_DIR"
echo "Upload all files from this folder to MonsterASP /wwwroot via SFTP/WebFTP."
