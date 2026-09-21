# CSharp dotnet 10 Blazor Server App Template

C# .NET Blazor server quick start template

## Quick Start

```bash
docker compose up
# or docker compose up --build

open http://localhost/

docker compose down
```

## Docker Run

```bash
docker build --check .
# Check complete, no warnings found.

docker build -t my-blazor-app .

docker run -p 80:8080 --rm \
  -e "DataProtectionSettings__AppSecretKey=local-dummy-key" \
  --name app my-blazor-app

open http://localhost/
```

* note: mount volume will work only for local
  - `-v "$HOME/.aspnet/DataProtection-Keys:/root/.aspnet/DataProtection-Keys"`

## Init Project Notes

```bash
dotnet --version
# 10.0.400

dotnet new update
# All template packages are up-to-date.

mkdir src tests

# add projects
dotnet new blazor -n MyApp.Web -o src/MyApp.Web --interactivity Server
dotnet new classlib -n MyApp.Core -o src/MyApp.Core
dotnet new classlib -n MyApp.Data -o src/MyApp.Data
dotnet new mstest -n MyApp.Tests -o tests/MyApp.Tests

# add reference
dotnet add src/MyApp.Web reference src/MyApp.Core
dotnet add src/MyApp.Web reference src/MyApp.Data
dotnet add src/MyApp.Data reference src/MyApp.Core

dotnet add tests/MyApp.Tests reference src/MyApp.Web
dotnet add tests/MyApp.Tests reference src/MyApp.Core
dotnet add tests/MyApp.Tests reference src/MyApp.Data

# add solution
dotnet new sln -n MyApp
dotnet sln MyApp.slnx add src/MyApp.Web
dotnet sln MyApp.slnx add src/MyApp.Core
dotnet sln MyApp.slnx add src/MyApp.Data
dotnet sln MyApp.slnx add tests/MyApp.Tests
```

* create misc configs

```bash
# create UseArtifactsOutput true in Directory.Build.props
dotnet new buildprops --use-artifacts

# to ensure dotnet version
dotnet new globaljson
```

* fix crlf to lf (note: dotnet new blazor generate crlf files)

```bash
# probably unset is recommended
git config --global --unset core.autocrlf

# find crlf
git ls-files --eol | grep crlf  # i/crlf  w/crlf ...

echo "* text=auto" > .gitattributes

# renormalize update index files to lf
git add --renormalize .         # i/lf    w/crlf

# stash and pop update worktree files to lf
git stash
git stash pop                   # i/crlf  w/lf

# update index files again to lf
git add --renormalize .         # i/lf    w/lf

git commit -m "chore: convert crlf to lf"
```

* add packages

```bash
# SonarQube
dotnet add src/MyApp.Web package SonarAnalyzer.CSharp

# DI analyzer
dotnet add src/MyApp.Web package DependencyInjection.Lifetime.Analyzers

# move these common ItemGroup to Directory.Build.props
```

```bash
# coverage in MTP
dotnet add tests/MyApp.Tests package Microsoft.Testing.Extensions.CodeCoverage

# bunit: Blazor UI Component Test Framework
dotnet add tests/MyApp.Tests package bunit

# https://bunit.dev/docs/test-doubles/mocking-httpclient.html
dotnet add tests/MyApp.Tests package RichardSzalay.MockHttp

# mock
dotnet add tests/MyApp.Tests package NSubstitute
```

* add tools

```bash
# to ensure tools version
dotnet new tool-manifest

dotnet tool install --local dotnet-reportgenerator-globaltool

# install and init husky
dotnet tool install --local Husky
dotnet husky install
# Git hooks installed

dotnet husky add pre-commit -c "dotnet format --verify-no-changes --severity info"
```

* Fluent UI Blazor
  - https://github.com/microsoft/fluentui-blazor

```bash
# dotnet templates for new app
dotnet new install Microsoft.FluentUI.AspNetCore.Templates

dotnet new fluentblazor --name MyApplication --interactivity Server
```

```bash
dotnet add src/MyApp.Web package Microsoft.FluentUI.AspNetCore.Components
dotnet add src/MyApp.Web package Microsoft.FluentUI.AspNetCore.Components.Icons
dotnet add src/MyApp.Web package Microsoft.FluentUI.AspNetCore.Components.Emoji
```

* run format test

```bash
# check vulnerable packages
dotnet list package --vulnerable --include-transitive

# check format
dotnet format --verify-no-changes --severity info

# check lint
dotnet clean && dotnet build

# run tests with coverage
dotnet test -- --coverage --coverage-output-format cobertura --coverage-settings coverage.settings.xml
# ./TestResults/<guid>.cobertura.xml

# generate coverage report
# warning: "TestResults/*.cobertura.xml" will merge all xml results
dotnet tool run reportgenerator \
  -reports:"TestResults/*.cobertura.xml" \
  -targetdir:"TestResults/html" \
  -reporttypes:Html \
  --minimumCoverageThresholds:lineCoverage=80 \
  --minimumCoverageThresholds:branchCoverage=80

open TestResults/html/index.html

# run tests with auto rerun
dotnet watch test --project tests/MyApp.Tests
```

* run dev server

```bash
# dev server with auto reload
dotnet watch run --project src/MyApp.Web
```

* upgrade packages

```bash
dotnet package list --outdated

dotnet package update
```

* setup local https

```bash
# setup dev certificate
dotnet dev-certs https --trust
# Trusting the HTTPS development certificate was requested. If the certificate is not already trusted we will run the following command:
# 'security add-trusted-cert -p basic -p ssl -k <<login-keychain>> <<certificate>>'
# This command might prompt you for your password to install the certificate on the keychain. To undo these changes: 'security remove-trusted-cert <<certificate>>'

# Successfully trusted the existing HTTPS certificate.

dotnet watch --project src/MyApp.Web --launch-profile https
```

## Trouble shooting

* `error CHARSET: Fix file encoding.`
  * select Save with Encoding `UTF-8 with BOM` → `UTF-8` in vscode
* dup entry in coverage report
  * clear cache: `rm -fr ./TestResults/*`
