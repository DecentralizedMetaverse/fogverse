using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DB_Sound", menuName = "DB/DB_Sound")]
public class DB_Sound : ScriptableObject
{
    public DB_SoundE[] data;
}

[System.Serializable]
public class DB_SoundE
{
    public string name;
    public string filePath;
    public float startTime;
    public float endTime;
}
