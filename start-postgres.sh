#!/bin/bash
set -e

CONTAINER="golfmanager-postgres"

if docker start "$CONTAINER" 2>/dev/null; then
    echo "Started existing postgres container."
else
    echo "Creating postgres container..."
    docker run -d \
        --name "$CONTAINER" \
        -e POSTGRES_USER=postgres \
        -e POSTGRES_PASSWORD=postgres \
        -e POSTGRES_DB=golfmanager \
        -e POSTGRES_HOST_AUTH_METHOD=trust \
        -p 5432:5432 \
        postgres:14-alpine
    echo "Postgres container created and running."
fi
