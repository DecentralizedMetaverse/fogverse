using System;
using System.Collections.Generic;
using MemoryPack;
using UnityEngine;

[MemoryPackable]
public partial class P_ObjectInfoResponse
{
    public string objId { get; set; }
    public string cid { get; set; }
    public string objName { get; set; }
    public Vector3 position { get; set; }
    public Vector3 rotation { get; set; }
    public List<string> components { get; set; }
    public List<byte[]> childObjId { get; set; }
}
