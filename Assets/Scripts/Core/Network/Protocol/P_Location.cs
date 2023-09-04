using System;
using System.Collections.Generic;
using MemoryPack;
using UnityEngine;

[MemoryPackable]
public partial class P_Location
{
    public string objId { get; set; }
    public Vector3 position { get; set; }
    public Vector3 rotation { get; set; }
}
