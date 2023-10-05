using System;
using System.Collections.Generic;
using MemoryPack;
using UnityEngine;

[MemoryPackable]
public partial class P_GetUserListResponse
{
    public List<string> ids { get; set; }
    public string chunk { get; set; }
}
