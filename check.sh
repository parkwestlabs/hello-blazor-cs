#!/bin/bash
set -euxo pipefail

dotnet format --verify-no-changes --severity info

dotnet clean && dotnet build

dotnet test -- \
  --coverage \
  --coverage-output-format cobertura \
  --coverage-output coverage.cobertura.xml \
  --coverage-settings coverage.settings.xml

dotnet tool run reportgenerator \
  -reports:"TestResults/coverage.cobertura.xml" \
  -targetdir:"TestResults/html" \
  -reporttypes:Html \
  --minimumCoverageThresholds:lineCoverage=80 \
  --minimumCoverageThresholds:branchCoverage=80
