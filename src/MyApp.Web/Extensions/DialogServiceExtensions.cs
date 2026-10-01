using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using FluentIcons = Microsoft.FluentUI.AspNetCore.Components.Icons.Filled.Size24;

namespace MyApp.Web.Extensions;

public static class DialogServiceExtensions
{
    /// <summary>
    /// 削除確認用の警告ダイアログ（OK/キャンセル）を表示します。XSS対策済み。
    /// </summary>
    /// <param name="dialogService">IDialogServiceのインスタンス</param>
    /// <param name="itemName">削除対象のアイテム名</param>
    /// <returns>ユーザーがOKを押した場合は true、キャンセルまたは閉じた場合は false</returns>
    public static async Task<bool> ShowDeleteConfirmationAsync(this IDialogService dialogService, string itemName)
    {
        // CA1062 null check
        ArgumentNullException.ThrowIfNull(dialogService);

        // 1. 動的なアイテム名を安全にエスケープ（XSS対策）
        string safeItemName = WebUtility.HtmlEncode(itemName);

        // 2. パラメータの組み立て
        var parameters = new DialogParameters<MessageBoxContent>()
        {
            Content = new MessageBoxContent()
            {
                Title = "削除の確認",
                // アイテム名を太字で強調する HTML マークアップ
                MarkupMessage = new MarkupString($"「<strong>{safeItemName}</strong>」を削除してもよろしいですか？"),
                Icon = new FluentIcons.Warning(),
                IconColor = Color.Warning,
            },
            PrimaryAction = "削除する",
            SecondaryAction = "キャンセル",
            PreventDismissOnOverlayClick = true
        };

        // 3. ダイアログを表示して結果を待機
        var dialog = await dialogService.ShowMessageBoxAsync(parameters);
        var result = await dialog.Result;

        // 4. 結果を bool 型（true / false）に変換して返却
        // Cancelled（キャンセルされた、またはESCで閉じられた）でなければ true
        return !result.Cancelled;
    }
}
