using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// ゲーム管理に必要なデータをまとめたクラス
/// </summary>
[CreateAssetMenu(fileName = "DB_GameManager", menuName = "DB/DB_GameManager")]
public class DB_GameManager : ScriptableObject
{
    [Header("Debugging Options")]
    public bool autoConnect = true;
    public bool autoClientMode = false;
    public bool visiblePerformance;
    public bool visibleLog;
    public bool startWorldPosition = true;
    public bool bgmOff = true;

    [Header("Debugging Authentication")]
    public string userName;
    public string password;
    public bool skipSignInScreen = true;

    [Header("Other")]
    public string metaPath = "../meta";
    public string contentPath = "../content";
    public string avatarPath = "../avatar";
    public string outputPath = "../output";

    //public eScene.Scene scene;
    [Header("Scene Group")]
    public eScene.Scene firstScene;
    public List<DB_GameManagerE> data = new(10);

#if UNITY_EDITOR
    public void SetSceneName()
    {
        // Sceneの名前を取得する
        foreach (var d in data)
        {
            var count = d.scene.Count;
            d.sceneName = new string[count];

            for (int i = 0; i < d.scene.Count; i++)
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
        CreateDirectory($"{Application.dataPath}/{outputPath}");
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

