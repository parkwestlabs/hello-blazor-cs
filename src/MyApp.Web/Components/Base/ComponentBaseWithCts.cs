using Microsoft.AspNetCore.Components;

namespace MyApp.Web.Components.Base;

// 共通の親コンポーネントクラスを作成
public abstract class ComponentBaseWithCts : ComponentBase, IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private bool _disposed;

    // 子コンポーネントで使う Token
    protected CancellationToken CancellationToken => _cts.Token;

    // 1. 公開用の Dispose メソッド
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // 2. 派生クラスでオーバーライド可能な Dispose(bool) メソッド
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // マネージドリソース（CancellationTokenSource）の破棄
                _cts.Cancel();
                _cts.Dispose();
            }

            _disposed = true;
        }
    }
}
