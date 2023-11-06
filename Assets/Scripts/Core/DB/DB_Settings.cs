using DC;
using UnityEngine;

[CreateAssetMenu(fileName = "DB_Settings", menuName = "DB/DB_Settings")]
public class DB_Settings : ScriptableObject
{
    public DB_SettingsData data = new DB_SettingsData();

    public int screenRes
    {
        get { return data.screenRes; }
        set
        {
            data.screenRes = value;
            Screen.SetResolution(fg.screenW[data.screenRes], fg.screenH[data.screenRes], data.fullScreen);
        }
    }

    public bool fullScreen
    {
        get { return data.fullScreen; }
        set
        {
            data.fullScreen = value;
            Screen.fullScreen = data.fullScreen;
        }
    }

    public void ReadSettings()
    {
        var data = GM.Msg<object>("GetSaveData", "settings");
        if (data == null) { return; }

        var txt = data.ToString();
        data = JsonUtility.FromJson<DB_SettingsData>(txt);
    }

    public void WriteSettings()
    {
        var txt = JsonUtility.ToJson(data);
        GM.Msg("SetSaveData", "settings", txt);
    }
}

[System.Serializable]
public class DB_SettingsData
{
    public string version;
    public string password;
    [Range(1f, 8f)]
    public float cameraSpeed;
    [Range(0f, 2f)]
    public float voiceVolume;
    public int deviceNum;
    public int screenRes;
    public bool fullScreen;
    [Range(0f, 1f)]
    public float bgmVolume;
    [Range(0f, 1f)]
    public float seVolume;
}