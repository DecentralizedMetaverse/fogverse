using System.IO;
using System.Security.Cryptography;
using System.Text;

public class CryptoText
{
    private static readonly string Password = "FEgeKJw$\"\"wef";
    private static readonly string SaltStr = "65wfeSgffw";
    private static readonly int Iterations = 10;
    private readonly byte[] _salt = Encoding.UTF8.GetBytes(SaltStr);

    /// <summary>
    /// Textを暗号化してbytesを返す
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public byte[] EncryptTextToBytes(string text)
    {
        var inputBytes = Encoding.UTF8.GetBytes(text);

        var keyDerive = new Rfc2898DeriveBytes(Password, _salt, Iterations, HashAlgorithmName.SHA256);
        var key = keyDerive.GetBytes(32);
        var iv = keyDerive.GetBytes(16);

        var aes = Aes.Create();

        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(key, iv), CryptoStreamMode.Write))
        {
            cs.Write(inputBytes, 0, inputBytes.Length); // データを書き込む
            cs.FlushFinalBlock(); // 最終ブロックの処理
        }

        return ms.ToArray();
    }

    /// <summary>
    /// Bytesを復号化してTextを返す
    /// </summary>
    /// <param name="encryptedData"></param>
    /// <returns></returns>
    public string DecryptBytesToText(byte[] encryptedData)
    {
        var keyDerive = new Rfc2898DeriveBytes(Password, _salt, Iterations, HashAlgorithmName.SHA256);
        var key = keyDerive.GetBytes(32);
        var iv = keyDerive.GetBytes(16);

        var aes = Aes.Create();

        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms,
                   aes.CreateDecryptor(key, iv), CryptoStreamMode.Write))
        {
            cs.Write(encryptedData, 0, encryptedData.Length); // データを書き込む
            cs.FlushFinalBlock(); // 最終ブロックの処理
        }

        var decryptedData = ms.ToArray(); // 復号化されたデータを取得する

        return Encoding.UTF8.GetString(decryptedData);
    }
}
