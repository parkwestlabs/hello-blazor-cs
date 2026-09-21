
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace MyApp.Web.Components.Base;

public abstract class NotificationPageBase : ComponentBase
{
    [Inject]
    private ILogger<NotificationPageBase> Logger { get; set; } = default!;

    [Inject]
    private IToastService ToastService { get; set; } = default!;

    // 💡 画面のあらゆる非同期アクションを安全に包み込む汎用メソッド
    protected async Task ExecuteSafeAsync(
        Func<Task> action,
        Action? onError = null)
    {
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            await action();
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            string pageName = GetType().Name;
            string actionName = action.Method.Name;
            string msg = ex.Message;
            Logger.LogError(ex, "Error in {PageName}/{ActionName}: {Msg}", pageName, actionName, msg);

            string userMessage = GetUserFriendlyMessage(ex);
            ToastService.ShowError(userMessage);

            onError?.Invoke();
        }
    }

    // ★ 集約エラーハンドリング（FastAPI風）
    private static string GetUserFriendlyMessage(Exception ex)
    {
        return ex switch
        {
            TimeoutException
                => "接続がタイムアウトしました。時間をおいて再度お試しください。",
            HttpRequestException
                => "通信エラーが発生しました。時間をおいて再度お試しください。",
            InvalidOperationException
                => "不正な操作が行われました。",
            _ => "予期しないエラーが発生しました。管理者にお問い合わせください。"
        };
    }
}
