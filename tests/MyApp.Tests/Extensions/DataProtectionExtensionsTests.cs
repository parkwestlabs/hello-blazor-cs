using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using MyApp.Web.Extensions;

namespace MyApp.Tests.Extensions;

[TestClass]
[SuppressMessage("DependencyInjection", "DI007:Consider injecting 'IDataProtectionProvider' directly instead of resolving via IServiceProvider")]
public class DataProtectionExtensionsTests
{
    private const string TestSecretKey = "my-super-secure-prod-secret-key-1234567890";

    /// <summary>
    /// 別々のサービスプロバイダー（別コンテナを疑似再現）であっても、
    /// 同じシークレットキーが渡されていれば、お互いに暗号化・復号が成功することを確認します。
    /// </summary>
    [TestMethod]
    public void UseSimpleCryptoTokenProvider_Should_AllowCrossInstanceDecryption_WithSameSecret()
    {
        // Arrange: 1つ目のコンテナインスタンスを模してDataProtectionを構築
        var services1 = new ServiceCollection();
        services1.AddDataProtection()
                 .SetApplicationName("TestApp")
                 .UseSimpleCryptoTokenProvider(TestSecretKey);
        using var provider1 = services1.BuildServiceProvider();

        var protector1 = provider1
            .GetRequiredService<IDataProtectionProvider>()
            .CreateProtector("Antiforgery");

        // Arrange: 2つ目のコンテナインスタンス（再起動後、またはオートスケールした別コンテナ）を模して構築
        var services2 = new ServiceCollection();
        services2.AddDataProtection()
                 .SetApplicationName("TestApp")
                 .UseSimpleCryptoTokenProvider(TestSecretKey);
        using var provider2 = services2.BuildServiceProvider();

        var protector2 = provider2.GetRequiredService<IDataProtectionProvider>().CreateProtector("Antiforgery");

        const string originalText = "Hello Cloud Run!";

        // Act: インスタンス1で暗号化する
        string cipherText = protector1.Protect(originalText);

        // Assert: インスタンス1で暗号化した文字列を、完全に独立したインスタンス2で正常に復号できるか検証
        string decryptedText = protector2.Unprotect(cipherText);

        Assert.AreEqual(originalText, decryptedText);
    }

    /// <summary>
    /// 異なるシークレットキーが渡された場合、暗号化の同期が取れず、
    /// 不正なトークンとして適切にブロック（例外スロー）されることを確認します。
    /// </summary>
    [TestMethod]
    public void UseSimpleCryptoTokenProvider_Should_FailDecryption_WithDifferentSecret()
    {
        // Arrange: 正しいキーを持つコンテナ
        var services1 = new ServiceCollection();
        services1.AddDataProtection().SetApplicationName("TestApp").UseSimpleCryptoTokenProvider(TestSecretKey);
        using var provider1 = services1.BuildServiceProvider();
        var protector1 = provider1.GetRequiredService<IDataProtectionProvider>().CreateProtector("Antiforgery");

        // Arrange: 異なるキー（または設定ミス）を持つコンテナ
        var services2 = new ServiceCollection();
        services2.AddDataProtection().SetApplicationName("TestApp").UseSimpleCryptoTokenProvider("wrong-secret-key-abcde");
        using var provider2 = services2.BuildServiceProvider();
        var protector2 = provider2.GetRequiredService<IDataProtectionProvider>().CreateProtector("Antiforgery");

        // Act: 正しいキーで暗号化
        string cipherText = protector1.Protect("Secret Data");

        // Assert: 違うキーのコンテナで復号しようとすると、安全に CryptographicException が発生することを検証
        Assert.Throws<System.Security.Cryptography.CryptographicException>(() =>
        {
            protector2.Unprotect(cipherText);
        });
    }

    /// <summary>
    /// 引数に null が渡された際、CA1062に基づくガード句が正しく働き、
    /// ArgumentNullExceptionを投げて即座に安全にクラッシュすることを確認します。
    /// </summary>
    [TestMethod]
    public void UseSimpleCryptoTokenProvider_Should_Throw_When_Argument_Is_Null()
    {
        var services = new ServiceCollection();
        var builder = services.AddDataProtection();

        // secretKey に null を渡した場合の検証
        Assert.Throws<ArgumentNullException>(() =>
        {
            builder.UseSimpleCryptoTokenProvider(null!);
        });
    }
}
