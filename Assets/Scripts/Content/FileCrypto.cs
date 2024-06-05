using System;
using System.IO;
using System.Security.Cryptography;
using Cysharp.Threading.Tasks;
using DC;
using UnityEngine;

/// <summary>
/// Fileの暗号化を行う
/// </summary>
public class FileCrypto : MonoBehaviour
{
    const string saltStr = "adfweffwaefwa";
    const int iterations = 100;
    byte[] salt;

    void Start()
    {
        salt = System.Text.Encoding.UTF8.GetBytes(saltStr);
        GM.Add<string, bool>("EncryptFile", EncryptFile);
        GM.Add<string, bool>("DecryptFile", DecryptFile);
        GM.Add<string, string, bool>("EncryptFileWithPassword", EncryptFileWithPassword);
        GM.Add<string, string, bool>("DecryptFileWithPassword", DecryptFileWithPassword);
        GM.Add<string, string, UniTask<byte[]>>("GetDecryptDataWithPassword", GetDecryptDataWithPassword);
        GM.Add<byte[], string, byte[], byte[]>("Encrypt", Encrypt);
        GM.Add<byte[], string, byte[], byte[]>("Decrypt", Decrypt);
    }

    /// <summary>
    /// 暗号化
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    bool EncryptFile(string path)
    {
        return EncryptFileWithPassword(path, GM.password);
    }

    bool EncryptFileWithPassword(string path, string password)
    {
        // Fileの内容をバイト配列として読み込む
        byte[] inputBytes = File.ReadAllBytes(path);

        byte[] encryptedData = Encrypt(inputBytes, password, salt);

        // 暗号化されたデータを別のFileに書き込む（拡張子は.encにする）
        var newFileName = path + ".enc";
        File.WriteAllBytes(newFileName, encryptedData);

        return true;
    }

    /// <summary>
    /// 復号化
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    bool DecryptFile(string path)
    {
        return DecryptFileWithPassword(path, GM.password);
    }

    bool DecryptFileWithPassword(string path, string password)
    {
        // 出力するFile名
        string outputFileName = path.Substring(0, path.LastIndexOf('.'));

        // 暗号化されたFileからソルトとデータを読み込む
        byte[] encryptedData = File.ReadAllBytes(path);

        byte[] decryptedData = Decrypt(encryptedData, password, salt);

        // 復号化されたデータを別のFileに書き込む
        File.WriteAllBytes(outputFileName, decryptedData);

        return true;
    }

    async UniTask<byte[]> GetDecryptDataWithPassword(string path, string password)
    {
        if (!File.Exists(path)) { return null; }

        while (IsFileLocked(path))
        {
            // Fileが使用中
            await UniTask.Yield();
        }
        
        // 暗号化されたFileからDataを読み込む
        var encryptedData = File.ReadAllBytes(path);

        return Decrypt(encryptedData, password, salt);
    }

    bool IsFileLocked(string path)
    {
        FileStream stream = null;
        try
        {
            stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        catch
        {
            return true;
        }
        finally
        {
            if (stream != null)
            {
                stream.Close();
            }
        }

        return false;
    }

    private byte[] Encrypt(byte[] inputBytes, string password, byte[] salt)
    {
        Rfc2898DeriveBytes keyDerive = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        byte[] key = keyDerive.GetBytes(32);
        byte[] iv = keyDerive.GetBytes(16);

        // AES暗号化Objectを作る
        Aes aes = Aes.Create();

        // 暗号化されたデータを格納するバイト配列を作る
        byte[] encryptedData;

        // 暗号化ストリームを作る
        using (MemoryStream ms = new MemoryStream())
        {
            // ms.Write(salt, 0, salt.Length); // ソルトを書き込む

            using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(key, iv), CryptoStreamMode.Write))
            {
                cs.Write(inputBytes, 0, inputBytes.Length); // データを書き込む
                cs.FlushFinalBlock(); // 最終ブロックの処理
            }

            encryptedData = ms.ToArray(); // 暗号化されたデータを取得する
        }

        return encryptedData;
    }

    private byte[] Decrypt(byte[] encryptedData, string password, byte[] salt)
    {
        //Array.Copy(encryptedData, 0, salt, 0, salt.Length);
        //byte[] data = new byte[encryptedData.Length - salt.Length];
        //Array.Copy(encryptedData, salt.Length, data, 0, data.Length);

        // パスワードとソルトからRfc2898DeriveBytesObjectを作る
        Rfc2898DeriveBytes keyDerive = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        byte[] key = keyDerive.GetBytes(32);
        byte[] iv = keyDerive.GetBytes(16);

        // AesObjectを作る
        Aes aes = Aes.Create();

        // 復号化されたデータを格納するバイト配列を作る
        byte[] decryptedData = null;

        // 復号化ストリームを作る
        try
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms,
                    aes.CreateDecryptor(key, iv), CryptoStreamMode.Write))
                {
                    cs.Write(encryptedData, 0, encryptedData.Length); // データを書き込む
                    cs.FlushFinalBlock(); // 最終ブロックの処理
                }

                decryptedData = ms.ToArray(); // 復号化されたデータを取得する
            }
        }
        catch (Exception e)
        {
            GM.LogWarning(e.Message);
            GM.Msg("ShortMessage", "Password is incorrect.");
        }

        return decryptedData;
    }
}
