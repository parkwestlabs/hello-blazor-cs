#!/bin/bash
set -euxo pipefail

dotnet format --verify-no-changes --severity info

dotnet clean && dotnet build

dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings

dotnet tool run reportgenerator \
  -reports:"**/coverage.cobertura.xml" \
  -targetdir:"coveragereport" \
  -reporttypes:Html \
  --minimumCoverageThresholds:lineCoverage=80 \
  --minimumCoverageThresholds:branchCoverage=80
