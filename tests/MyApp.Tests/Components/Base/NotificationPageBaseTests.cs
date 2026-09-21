using Bunit;
using NSubstitute;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyApp.Web.Components.Base;
using System.Diagnostics.CodeAnalysis;

namespace MyApp.Tests.Components.Base;

[TestClass]
public class NotificationPageBaseTests
{
    private readonly ILogger<NotificationPageBase> _mockLogger;
    private readonly IToastService _mockToastService;

    public NotificationPageBaseTests()
    {
        _mockLogger = Substitute.For<ILogger<NotificationPageBase>>();
        _mockToastService = Substitute.For<IToastService>();
    }

    /// <summary>
    /// テスト用に NotificationPageBase を継承した具象コンポーネント
    /// </summary>
    [SuppressMessage("Performance", "CA1812:インスタンス化されていない内部クラスを回避する", Justification = "bUnitによって動的にインスタンス化されます")]
    private sealed class TestNotificationPage : NotificationPageBase
    {
        // テストコードから内部の ExecuteSafeAsync を呼び出せるように公開する
        public async Task TestExecuteSafeAsync(Func<Task> action)
        {
            await ExecuteSafeAsync(action);
        }
    }

    /// <summary>
    /// 共通のDI登録処理をまとめるヘルパーメソッド
    /// </summary>
    private void ConfigureServices(BunitContext ctx)
    {
        ctx.Services.AddSingleton(_mockLogger);
        ctx.Services.AddSingleton(_mockToastService);
    }

    [TestMethod]
    public async Task ExecuteSafeAsync_Success_DoesNotLogOrShowToast()
    {
        using var ctx = new BunitContext();
        ConfigureServices(ctx);

        // Arrange: テスト用コンポーネントをレンダリング
        var cut = ctx.Render<TestNotificationPage>();
        var actionExecuted = false;

        // Act: 正常に終了するアクションを実行
        await cut.Instance.TestExecuteSafeAsync(() =>
        {
            actionExecuted = true;
            return Task.CompletedTask;
        });

        // Assert: アクションが実行され、エラー処理が動いていないことを検証
        Assert.IsTrue(actionExecuted);
        _mockToastService.DidNotReceive().ShowError(Arg.Any<string>());
    }

    [TestMethod]
    [DataRow(typeof(TimeoutException), "接続がタイムアウトしました")]
    [DataRow(typeof(HttpRequestException), "通信エラーが発生しました")]
    [DataRow(typeof(InvalidOperationException), "不正な操作が行われました")]
    [DataRow(typeof(Exception), "予期しないエラーが発生しました")]
    public async Task ExecuteSafeAsync_OnException_LogsErrorAndShowsToast(
        Type exceptionType, string expectedMessage
    )
    {
        // Arrange
        using var ctx = new BunitContext();
        ConfigureServices(ctx);

        var cut = ctx.Render<TestNotificationPage>();

        var expectedException = (Exception)Activator.CreateInstance(exceptionType, "テスト用エラー")!;

        // Act: 例外を投げるアクションを実行
        await cut.Instance.TestExecuteSafeAsync(() => throw expectedException);

        // Assert 1: ILogger.LogError が呼び出されたことを検証
        _mockLogger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<Arg.AnyType>(),
            expectedException,
            Arg.Any<Func<Arg.AnyType, Exception?, string>>());

        // Assert 2: 各例外に応じた適切なトーストメッセージが表示されたことを検証
        _mockToastService.Received(1)
            .ShowError(Arg.Is<string>(msg => msg.Contains(expectedMessage)));
    }

    [TestMethod]
    public async Task ExecuteSafeAsync_WhenActionIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        using var ctx = new BunitContext();
        ConfigureServices(ctx);

        var cut = ctx.Render<TestNotificationPage>();

        // Act & Assert: null を渡した時に ArgumentNullException が発生することを検証 (CA1062の担保)
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await cut.Instance.TestExecuteSafeAsync(null!);
        });
    }
}
