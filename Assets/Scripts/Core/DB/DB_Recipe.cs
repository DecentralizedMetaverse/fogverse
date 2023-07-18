using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DB_Recipe", menuName = "DB/DB_Recipe")]
public class DB_Recipe : ScriptableObject
{
    public DB_RecipeE[] data;
}
[System.Serializable]
public class DB_RecipeE
{
    public byte ctID;
    public byte id;

    public bool learned;

    public byte[] rctID;
    public byte[] rID;
}
