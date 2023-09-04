using System;
using System.Collections.Generic;
using MemoryPack;
using UnityEngine;

[MemoryPackable]
public partial class P_ChangeNametag
{
    public string objId { get; set; }
    public string nametag { get; set; }
}
