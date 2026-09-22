# CSharp dotnet 10 Blazor Server App Template

C# .NET Blazor server quick start template

## Quick Start

```bash
docker compose up
# or docker compose up --build

open http://localhost/

# clean up
docker compose ps -a
docker compose down
```

## Local Dev Server

```bash
# run format lint and tests
./check.sh

# coverage report
open TestResults/html/index.html

# run tests with auto rerun
dotnet watch test --project tests/MyApp.Tests

# dev server with auto reload
dotnet watch run --project src/MyApp.Web
```

## Other Docs

* Init Blazor Server Project Notes
  - [docs/init-blazor-server.md](docs/init-blazor-server.md)
