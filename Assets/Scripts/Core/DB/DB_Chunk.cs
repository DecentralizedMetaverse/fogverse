using DC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DB_Chunk", menuName = "DB/DB_Chunk")]
public class DB_Chunk : ScriptableObject
{
    /// <summary>
    /// id, chunk
    /// </summary>
    public Dictionary<string, (int, int, int)> chunkTable = new(); 

    /// <summary>
    /// 座標を送信する対象のChunk
    /// </summary>
    public HashSet<(int, int, int)> sendTargetChunk = new();

    public int chunkSize = 16;
}
