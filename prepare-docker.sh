#!/bin/bash
set -e

echo "Pre-downloading NuGet packages for offline Docker build..."
dotnet restore src/GolfManager.Api/GolfManager.Api.csproj \
    --packages .nuget-docker-cache

echo ""
echo "Done. Run 'docker compose build' to build the image."
