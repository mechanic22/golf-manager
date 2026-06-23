#!/bin/bash
set -e

IMAGE_NAME="docker.5thbox.com/dkgolf/golfmanager-api"
DATE_TAG="$(date +'%Y%m%d%H%M')"

dotnet publish src/GolfManager.Api/GolfManager.Api.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained false \
    -o publish

docker build \
    --platform linux/amd64 \
    -f src/GolfManager.Api/Dockerfile \
    -t "$IMAGE_NAME:$DATE_TAG" \
    -t "$IMAGE_NAME:latest" \
    .

rm -rf publish

docker push "$IMAGE_NAME:$DATE_TAG"
docker push "$IMAGE_NAME:latest"

echo ""
echo "Pushed: $IMAGE_NAME:$DATE_TAG"
echo "Pushed: $IMAGE_NAME:latest"
