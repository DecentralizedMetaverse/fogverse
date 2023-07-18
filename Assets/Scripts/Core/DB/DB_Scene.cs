using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "DB_Scene", menuName = "DB/DB_Scene")]
public class DB_Scene : ScriptableObject
{
    public bool offlineMode = true;
    public bool dbServerOfflineMode = true;
    public bool skipSignInScreen = true;
    public bool startWorldPosition = true;
    public bool bgmOff = true;
    public List<DB_SceneE> data = new List<DB_SceneE>();
    public eScene.Scene scene;
    public eScene.Scene world;
}
[System.Serializable]
public class DB_SceneE
{
    public string name;
    public string[] scene = new string[4];
}

