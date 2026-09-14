using System.Text;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;

namespace MyApp.Web.Extensions;

/// <summary>
/// use in-memory xml instead of xml in $HOME/.aspnet/DataProtection-Keys
/// </summary>
public static class DataProtectionExtensions
{
    public static IDataProtectionBuilder UseSimpleCryptoTokenProvider(this IDataProtectionBuilder builder, string secretKey)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(secretKey);

        // 固定のシークレットキーから一意の256bit（32バイト）のマスターキーを生成
        byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey.PadRight(32)[..32]);

        builder.Services.PostConfigure<KeyManagementOptions>(options =>
        {
            // 鍵の自動生成(ローテーション)や有効期限による更新をすべて無効化
            options.AutoGenerateKeys = false;
            // デフォルトでは FileSystemXmlRepository が $HOME/.aspnet/DataProtection-Keys で
            // xml を読み書きするが、それを in-memory の xml に固定する
            options.XmlRepository = new FixedXmlRepository(keyBytes);
        });
        return builder;
    }
}

internal sealed class FixedXmlRepository : IXmlRepository
{
    private readonly string _xmlPayload;

    public FixedXmlRepository(byte[] keyBytes)
    {
        // 固定のバイト配列をBase64化し、DataProtectionが解読できるXML文字列を1つ生成
        string base64Key = Convert.ToBase64String(keyBytes);

        // 鍵を特定・同期するための一意の固定UUID (cf. AutoGenerateKeys false)
        string uuidStr = "951202F6-D166-4829-8943-0F9F18A2E622";

        // 有効期限を「遥か未来(9999年)」に固定した、静的な鍵XMLを定義
        _xmlPayload = $@"
        <key id=""{uuidStr}"" version=""1"">
          <creationDate>2026-01-01T00:00:00Z</creationDate>
          <activationDate>2026-01-01T00:00:00Z</activationDate>
          <expirationDate>9999-12-31T23:59:59Z</expirationDate>
          <descriptor deserializerType=""Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel.AuthenticatedEncryptorDescriptorDeserializer, Microsoft.AspNetCore.DataProtection"">
            <descriptor>
              <encryption algorithm=""AES_256_CBC"" />
              <validation algorithm=""HMACSHA256"" />
              <masterKey enc:requiresEncryption=""false"" xmlns:enc=""http://microsoft.com"" >{base64Key}</masterKey>
            </descriptor>
          </descriptor>
        </key>";
    }

    public IReadOnlyCollection<XElement> GetAllElements()
    {
        // 常にこの1つの固定鍵だけをシステムに認識させる
        return new ReadOnlyCollection<XElement>([XElement.Parse(_xmlPayload)]);
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        // 新しい鍵の保存（書き込み）要求はすべて無視（スルー）する
    }
}
