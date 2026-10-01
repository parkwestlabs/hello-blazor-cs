# Setup EF DbUp with Local PostgreSQL

## Docker Postgres

```bash
docker compose up postgres pgweb -d
docker compose ps -a

# pgweb: Postgres admin app
open http://localhost:8081
```

## DB Migration

```bash
# マイグレーションの作成 (Add-Migration に対応)
dotnet ef migrations add InitialCreate \
  --project src/MyApp.Data --startup-project src/MyApp.Web

# データベースへの反映 (Update-Database に対応)
# DbUp から更新するので database update は利用しない
# dotnet ef database update \
#   --project src/MyApp.Data --startup-project src/MyApp.Web

# migrations add の差分からSQLスクリプトファイルを生成
# 引数なしでは「最初（空のDB）から最新のマイグレーションまで」の通し SQL が生成される
dotnet ef migrations script \
  --output ./src/MyApp.Web/Scripts/$(date +'%Y%m%d%H%M')_CreateTables.sql \
  --project src/MyApp.Data --startup-project src/MyApp.Web

# 2回目以降の差分だけを出力したい場合は、「前回のマイグレーション名」を引数に指定する
# 例: 0001(Initial) から 0002(AddUserAge) への差分だけを抽出して出力
dotnet ef migrations script InitialCreate AddUserAge \
  --output ./src/MyApp.Web/Scripts/$(date +'%Y%m%d%H%M')_AddUserAge.sql \
  --project src/MyApp.Data --startup-project src/MyApp.Web
```

* やり直す場合

```bash
# データベースを削除
dotnet ef database drop \
  --project src/MyApp.Data --startup-project src/MyApp.Web

# DB data 全削除
docker compose down --volumes
```

## Setup Project

```bash
dotnet tool install --global dotnet-ef
# Tool 'dotnet-ef' (version '10.0.12') was successfully installed.

# required to use dotnet ef CLI
dotnet add src/MyApp.Web package Microsoft.EntityFrameworkCore.Design

dotnet add src/MyApp.Web package dbup-postgresql

dotnet add src/MyApp.Data package Npgsql.EntityFrameworkCore.PostgreSQL

dotnet add tests/MyApp.Tests package Microsoft.EntityFrameworkCore.InMemory
```
