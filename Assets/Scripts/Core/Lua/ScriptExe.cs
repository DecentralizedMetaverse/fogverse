using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Luaを実行する
/// </summary>
public class ScriptExe : MonoBehaviour
{
    [SerializeField] string code;

    public void Exe()
    {
        LuaManager.lua.DoString(code);
    }
}
