using System;
using System.Collections.Generic;
using MemoryPack;
using UnityEngine;

[MemoryPackable]
public partial class P_UserDataResponse
{
    public (int, int, int) chunk { get; set; }
}
