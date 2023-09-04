using MemoryPack;
using UnityEngine;

[MemoryPackable]
public partial class P_LoginResponse
{
    public bool success { get; set; }
    public string message { get; set; }
    public float time { get; set; }
}
