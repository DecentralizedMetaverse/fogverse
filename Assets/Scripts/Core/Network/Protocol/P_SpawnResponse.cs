using MemoryPack;
using UnityEngine;

[MemoryPackable]
public partial class P_SpawnResponse
{
    public bool success { get; set; }
    public string message { get; set; }
}
