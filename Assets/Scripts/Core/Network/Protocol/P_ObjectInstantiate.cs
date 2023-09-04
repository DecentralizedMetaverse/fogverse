using System;
using System.Collections.Generic;
using MemoryPack;
using UnityEngine;

[MemoryPackable]
public partial class P_ObjectInstantiate
{
    public string objId { get; set; }
    public string objName { get; set; }
    public string cid { get; set; }
    public Vector3 position { get; set; }
    public Vector3 rotation { get; set; }
    public string objType { get; set; }
    public string nameTag { get; set; }
}
