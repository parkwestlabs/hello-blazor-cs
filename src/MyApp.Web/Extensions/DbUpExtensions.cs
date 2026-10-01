using DbUp;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace MyApp.Web.Extensions;

[ExcludeFromCodeCoverage(Justification = "DbUpの起動用初期化コードであり、実DBを必要とするため統合テストで検証します。")]
public static class DbUpExtensions
{
    /// <summary>
    /// 🚀 データベースマイグレーション (DbUp) を実行します。
    /// </summary>
    public static WebApplication RunDatabaseMigrations(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var connectionString = app.Configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            app.Logger.LogWarning("DefaultConnection not found: skipping DB Migration");
            return app;
        }

#if DEBUG
        // CREATE DATABASE: 開発環境などで、データベース自体が存在しない場合は自動作成する
        EnsureDatabase.For.PostgresqlDatabase(connectionString);
#endif

        // DbUp のアップグレーダー（実行器）を構築
        var upgrader = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            // 埋め込みリソースから「Scripts」フォルダ配下の SQL ファイルを読み込む
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(),
                name => name.Contains(".Scripts.", StringComparison.Ordinal))
            // 実行ログを標準出力（コンソール）に表示する
            .LogToConsole()
            .Build();

        // 未適用のスクリプトがあるかチェック
        if (upgrader.IsUpgradeRequired())
        {
            app.Logger.LogInformation("DB Upgrade Required. DB Migration running...");

            // CREATE/ALTER TABLE: スキーマの更新（SQLスクリプトの実行）
            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                // マイグレーションに失敗した場合はアプリの起動を停止する
                app.Logger.LogCritical(result.Error, "DB Migration failed. Stopping application...");
                throw result.Error;
            }

            app.Logger.LogInformation("DB Migration Done.");
        }
        else
        {
            app.Logger.LogInformation("DB Migration not required. DB schema is up to date.");
        }

        return app;
    }
}
