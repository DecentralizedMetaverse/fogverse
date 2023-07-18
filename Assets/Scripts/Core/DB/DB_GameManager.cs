using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// ゲーム管理に必要なデータをまとめたクラス
/// </summary>
[CreateAssetMenu(fileName = "DB_GameManager", menuName = "DB/DB_GameManager")]
public class DB_GameManager : ScriptableObject
{    
    [Header("Debugging Options")]
    public bool visiblePerformance;
    public bool visibleLog;
    public bool startWorldPosition = true;
    public bool bgmOff = true;
    
    [Header("Debugging Authentication")] 
    public string userName;
    public string password;
    public bool skipSignInScreen = true;

    [Header("Other")]
    public int chunkSize = 256;
    public string metaPath = "StreamingAssets/meta";
    public string contentPath = "StreamingAssets/content";
    public string avatarPath = "StreamingAssets/avatar";

    //public eScene.Scene scene;
    [Header("Scene Group")]
    public eScene.Scene firstScene;
    public List<DB_GameManagerE> data = new(10);

#if UNITY_EDITOR
    public void SetSceneName()
    {
        // Sceneの名前を取得する
        foreach(var d in data)
        {
            var count = d.scene.Count;
            d.sceneName = new string[count];
            
            for(int i = 0; i < d.scene.Count; i++)
            {
                d.sceneName[i] = d.scene[i].name;
            }
        }
    }
#endif

    public void Init()
    {
        CreateDirectory($"{Application.dataPath}/{metaPath}");
        CreateDirectory($"{Application.dataPath}/{contentPath}");
        CreateDirectory($"{Application.dataPath}/{avatarPath}");

    }

    void CreateDirectory(string path)
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
    }
}

[System.Serializable]
public class DB_GameManagerE
{
    public string groupName;
#if UNITY_EDITOR
    public List<SceneAsset> scene = new(10);
#endif
    [HideInInspector] public string[] sceneName;
}

