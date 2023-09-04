using MemoryPack;
using UnityEngine;

[MemoryPackable]
public partial class P_LoginRequest
{
    public string username { get; set; }
    public string password { get; set; }
}
