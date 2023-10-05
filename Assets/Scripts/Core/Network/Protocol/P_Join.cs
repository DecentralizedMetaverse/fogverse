using System;
using System.Collections.Generic;
using MemoryPack;
using UnityEngine;

[MemoryPackable]
public partial class P_Join
{
    public List<string> joinIds { get; set; }
    public string chunk { get; set; }
}
