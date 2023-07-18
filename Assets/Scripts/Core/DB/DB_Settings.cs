using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DB_Settings", menuName = "DB/DB_Settings")]
public class DB_Settings : ScriptableObject
{
    public string version;
    public string password;
    [Range(1f, 8f)]
    public float cameraSpeed;
    [Range(0f, 2f)]
    public float voiceVolume;
    public int _screenRes;
    public bool _fullScreen;
    [Range(0f, 1f)]
    public float bgmVolume;
    [Range(0f, 1f)]
    public float seVolume;
    
    public int screenRes
    {
        get { return _screenRes; }
        set
        {
            _screenRes = value;
            Screen.SetResolution(fg.screenW[_screenRes], fg.screenH[_screenRes], _fullScreen);
        }
    }

    public bool fullScreen
    {
        get { return _fullScreen; }
        set
        {
            _fullScreen = value;
            Screen.fullScreen = _fullScreen;
        }
    }
}