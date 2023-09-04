using System;
using System.Collections.Generic;
using MemoryPack;
using UnityEngine;

[MemoryPackable]
public partial class P_ObjectResponse
{
    public string objId { get; set; }
    public string objName { get; set; }
    public string cid { get; set; }
}
