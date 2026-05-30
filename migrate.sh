#!/usr/bin/env bash
set -euo pipefail

# Paths relative to this script's location (repo root)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DATA_PROJECT="$SCRIPT_DIR/UBB-SE-2026-Hospital/Hospital.Data/Hospital.Data.csproj"

echo "==> Checking dotnet ef tool..."
if ! dotnet ef --version &>/dev/null; then
    echo "dotnet-ef not found — installing globally..."
    dotnet tool install --global dotnet-ef
fi

echo
echo "==> Cleaning previous build output..."
dotnet clean "$DATA_PROJECT" -c Debug --nologo -v quiet

echo
echo "==> Restoring NuGet packages..."
dotnet restore "$DATA_PROJECT"

echo
echo "==> Dropping database (if it exists)..."
dotnet ef database drop \
    --project "$DATA_PROJECT" \
    --startup-project "$DATA_PROJECT" \
    --force --verbose

echo
echo "==> Applying all migrations (database update)..."
dotnet ef database update \
    --project "$DATA_PROJECT" \
    --startup-project "$DATA_PROJECT" \
    --verbose

echo
echo "==> Done. Database is at the latest migration."
