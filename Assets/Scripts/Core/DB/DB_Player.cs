using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DB_Player", menuName = "DB/DB_Player")]

public class DB_Player : ScriptableObject
{
    public Transform worldRoot;
    public Transform user;
    public int chunkX;
    public int chunkY;
}
