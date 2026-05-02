#!/bin/bash

# Reset database and import Holy Grail data
# This script deletes the current database and triggers a fresh import

set -e

echo "========================================"
echo "Reset Database & Import Holy Grail Data"
echo "========================================"
echo ""

# Navigate to API directory
cd "$(dirname "$0")/src/GolfManager.Api"

# Check if database exists
if [ -f "GolfManager.db" ]; then
    echo "🗑️  Deleting existing database..."
    rm -f GolfManager.db*
    echo "✅ Database deleted"
else
    echo "ℹ️  No existing database found"
fi

# Check if backup file exists
if [ -f "DkGolf_Backup_202604270946.sql" ]; then
    echo "✅ Holy Grail backup file found"
    echo ""
    echo "📦 File size: $(du -h DkGolf_Backup_202604270946.sql | cut -f1)"
else
    echo "❌ ERROR: Holy Grail backup file not found!"
    echo "   Expected: $(pwd)/DkGolf_Backup_202604270946.sql"
    exit 1
fi

echo ""
echo "========================================"
echo "Ready to import!"
echo "========================================"
echo ""
echo "Next steps:"
echo "1. Start your API watcher (dotnet watch run)"
echo "2. The import will run automatically on startup"
echo "3. Watch the logs for import progress"
echo ""
echo "Expected imports:"
echo "  - 1 league (DigiKey Golf League)"
echo "  - 189 users (password: ChangeMe123!)"
echo "  - All courses, tees, holes"
echo "  - 7 seasons (2019-2025)"
echo "  - All events and match results"
echo ""
