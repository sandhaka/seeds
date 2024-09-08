#!/bin/bash

# Load .env variables
if [ -f .env ]; then
  export $(grep -v '^#' .env | xargs)
fi

rm -r ./nupkg/*
dotnet pack ./Monads/monads.csproj -o ./nupkg &&

dotnet nuget push ./nupkg/*.nupkg \
    --api-key $GITHUB_ACCESS_TOKEN \
    --source https://nuget.pkg.github.com/sandhaka/index.json
