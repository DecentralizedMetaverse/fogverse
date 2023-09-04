using System;
using System.Collections.Generic;
using MemoryPack;
using UnityEngine;

[MemoryPackable]
public partial class P_RPC
{
    public string method { get; set; }
    public string args { get; set; }
}
