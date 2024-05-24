using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using File = System.IO.File;

public class SaveData : IDisposable
{
    public static SaveData I { get; set; }

    private static readonly string Path = $"{Application.persistentDataPath}/Save.dat";

    private readonly CryptoText _cryptoText = new();
    private Dictionary<string, object> _data = new();

    internal SaveData()
    {
        I = this;
        Load();
    }

    public void Dispose()
    {
        Save();
    }

    public void Set<T>(string key, T value, bool save = true)
    {
        _data[key] = value;
        if (save) Save();
    }

    public bool TryGetValue<T>(string key, out T value) where T : IConvertible
    {
        if (_data.TryGetValue(key, out var obj))
        {
            value = (T)Convert.ChangeType(obj, typeof(T));
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetSerializedValue<T>(string key, out T value)
    {
        if (_data.TryGetValue(key, out var obj))
        {
            try
            {
                value = JsonConvert.DeserializeObject<T>(obj.ToString());
                return true;
            }
            catch (JsonException)
            {
                value = default;
                return false;
            }
        }

        value = default;
        return false;
    }

    public void Load()
    {
        if (!File.Exists(Path)) return;

        var bytes = File.ReadAllBytes(Path);
        var json = _cryptoText.DecryptBytesToText(bytes);
        _data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        Debug.Log($"[Loaded]\n{json}");
    }

    public void Save()
    {
        var json = JsonConvert.SerializeObject(_data);
        var bytes = _cryptoText.EncryptTextToBytes(json);
        File.WriteAllBytes(Path, bytes);
        Debug.Log($"[Saved]\n{json}");
    }
}
