#!/bin/bash

# Prevent Kestrel from trying to bind to port 5000 (which may be in use)
# by setting a temporary port during migration execution
export ASPNETCORE_URLS=http://localhost:5005

echo "Reverting database schema to 0 (undoing all applied migrations) for all DbContexts..."

contexts=(
    "PassContext"
    "PassTypeContext"
    "SkiCardContext"
    "ResortContext"
    "ApplicationUserContext"
)

# Navigate to the Web project directory
cd src/AlpineSkiHouse.Web

for context in "${contexts[@]}"
do
    echo ""
    echo "=================================================="
    echo "Reverting migrations to 0 for: $context"
    echo "=================================================="
    dotnet ef database update 0 --context $context
    
    if [ $? -ne 0 ]; then
        echo "Error: Revert failed for $context"
        exit 1
    fi
done

echo ""
echo "=================================================="
echo "SUCCESS: All DbContext migrations reverted to 0!"
echo "=================================================="
