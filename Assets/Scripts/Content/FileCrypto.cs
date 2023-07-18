using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using DC;
using UnityEngine;

/// <summary>
/// File�̈Í������s��
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
        GM.Add<byte[], string, byte[], byte[]>("Encrypt", Encrypt);
        GM.Add<byte[], string, byte[], byte[]>("Decrypt", Decrypt);
    }

    /// <summary>
    /// �Í���
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    bool EncryptFile(string path)
    {       
        return EncryptFileWithPassword(path, GM.password);
    }
    
    bool EncryptFileWithPassword(string path, string password)
    {
        // File�̓��e���o�C�g�z��Ƃ��ēǂݍ���
        byte[] inputBytes = File.ReadAllBytes(path);

        byte[] encryptedData = Encrypt(inputBytes, password, salt);

        // �Í������ꂽ�f�[�^��ʂ�File�ɏ������ށi�g���q��.enc�ɂ���j
        var newFileName = path + ".enc";
        File.WriteAllBytes(newFileName, encryptedData);

        return true;
    }

    /// <summary>
    /// ������
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    bool DecryptFile(string path)
    {
        return DecryptFileWithPassword(path, GM.password);
    }
    
    bool DecryptFileWithPassword(string path, string password)
    {
        // �o�͂���File��
        string outputFileName = path.Substring(0, path.LastIndexOf('.'));

        // �Í������ꂽFile����\���g�ƃf�[�^��ǂݍ���
        byte[] encryptedData = File.ReadAllBytes(path);

        byte[] decryptedData = Decrypt(encryptedData, password, salt);

        // ���������ꂽ�f�[�^��ʂ�File�ɏ�������
        File.WriteAllBytes(outputFileName, decryptedData);

        return true;
    }

    private byte[] Encrypt(byte[] inputBytes, string password, byte[] salt)
    {
        Rfc2898DeriveBytes keyDerive = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        byte[] key = keyDerive.GetBytes(32);
        byte[] iv = keyDerive.GetBytes(16);

        // AES�Í���Object�����
        Aes aes = Aes.Create();

        // �Í������ꂽ�f�[�^���i�[����o�C�g�z������
        byte[] encryptedData;

        // �Í����X�g���[�������
        using (MemoryStream ms = new MemoryStream())
        {
            // ms.Write(salt, 0, salt.Length); // �\���g����������

            using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(key, iv), CryptoStreamMode.Write))
            {
                cs.Write(inputBytes, 0, inputBytes.Length); // �f�[�^����������
                cs.FlushFinalBlock(); // �ŏI�u���b�N�̏���
            }

            encryptedData = ms.ToArray(); // �Í������ꂽ�f�[�^���擾����
        }

        return encryptedData;
    }

    private byte[] Decrypt(byte[] encryptedData, string password, byte[] salt)
    {
        //Array.Copy(encryptedData, 0, salt, 0, salt.Length);
        //byte[] data = new byte[encryptedData.Length - salt.Length];
        //Array.Copy(encryptedData, salt.Length, data, 0, data.Length);

        // �p�X���[�h�ƃ\���g����Rfc2898DeriveBytesObject�����
        Rfc2898DeriveBytes keyDerive = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        byte[] key = keyDerive.GetBytes(32);
        byte[] iv = keyDerive.GetBytes(16);

        // AesObject�����
        Aes aes = Aes.Create();

        // ���������ꂽ�f�[�^���i�[����o�C�g�z������
        byte[] decryptedData = null;

        // �������X�g���[�������
        try
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms,
                    aes.CreateDecryptor(key, iv), CryptoStreamMode.Write))
                {
                    cs.Write(encryptedData, 0, encryptedData.Length); // �f�[�^����������
                    cs.FlushFinalBlock(); // �ŏI�u���b�N�̏���
                }

                decryptedData = ms.ToArray(); // ���������ꂽ�f�[�^���擾����
            }
        }
        catch(Exception e)
        {
            GM.LogWarning(e.Message);
            GM.Msg("ShortMessage", "Password is incorrect.");
        }

        return decryptedData;
    }
}
