# CSharp dotnet 10 Blazor Server App Template

C# .NET Blazor server quick start template

## Init Project Notes

```bash
dotnet --version
# 10.0.400

dotnet new blazor -o BlazorHelloWorld --interactivity Server
dotnet new mstest -n BlazorHelloWorld.Tests

# add reference
dotnet add BlazorHelloWorld.Tests/BlazorHelloWorld.Tests.csproj reference BlazorHelloWorld/BlazorHelloWorld.csproj

dotnet new sln -n BlazorHelloWorld
dotnet sln add BlazorHelloWorld/BlazorHelloWorld.csproj
dotnet sln add BlazorHelloWorld.Tests/BlazorHelloWorld.Tests.csproj
```

* create misc configs

```bash
# create UseArtifactsOutput true in Directory.Build.props
dotnet new buildprops --use-artifacts

# to ensure dotnet version
dotnet new globaljson
```

* fix crlf to lf

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
cd BlazorHelloWorld/

# SonarQube
dotnet add package SonarAnalyzer.CSharp

# DI analyzer
dotnet add package DependencyInjection.Lifetime.Analyzers

# move these common ItemGroup to Directory.Build.props
```

```bash
cd BlazorHelloWorld.Tests/

# coverage in MTP
dotnet add package Microsoft.Testing.Extensions.CodeCoverage

# bunit: Blazor UI Component Test Framework
dotnet add package bunit

# mock
dotnet add package NSubstitute
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
dotnet watch test --project BlazorHelloWorld.Tests
```

* run dev server

```bash
# dev server with auto reload
dotnet watch run --project BlazorHelloWorld
```

* upgrade packages

```bash
dotnet package list --outdated

dotnet package update
```

## Trouble shooting

* `error CHARSET: Fix file encoding.`
  * select Save with Encoding `UTF-8 with BOM` → `UTF-8` in vscode
* dup entry in coverage report
  * clear cache: `rm -fr ./TestResults/*`
