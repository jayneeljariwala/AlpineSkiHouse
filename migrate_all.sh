#!/bin/bash

# Prevent Kestrel from trying to bind to port 5000 (which may be in use)
# by setting a temporary port during migration execution
export ASPNETCORE_URLS=http://localhost:5005

echo "Starting EF database updates for all DbContexts..."

contexts=(
    "ApplicationUserContext"
    "ResortContext"
    "SkiCardContext"
    "PassTypeContext"
    "PassContext"
)

# Navigate to the Web project directory
cd src/AlpineSkiHouse.Web

for context in "${contexts[@]}"
do
    echo ""
    echo "=================================================="
    echo "Running migrations for: $context"
    echo "=================================================="
    dotnet ef database update --context $context
    
    if [ $? -ne 0 ]; then
        echo "Error: Migration failed for $context"
        exit 1
    fi
done

echo ""
echo "=================================================="
echo "SUCCESS: All DbContext migrations updated successfully!"
echo "=================================================="
