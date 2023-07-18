using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lua‚ğÀs‚·‚é
/// </summary>
public class ScriptExe : MonoBehaviour
{
    [SerializeField] string code;

    public void Exe()
    {
        LuaManager.lua.DoString(code);
    }
}
