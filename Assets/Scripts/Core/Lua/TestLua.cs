using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using System.Text;
using System;
using System.IO;
using Cysharp.Threading.Tasks;

public class TestLua : MonoBehaviour
{    

    const string initFilePath = "Assets/LuaScripts/Core/init.lua";
    const string exeFilePath = "Assets/LuaScripts/Core/exe.lua";
    string initCode = "";
    string exeCode = "";

    string defineStart = "local co = coroutine.create(function()";
    string defineEnd = "end)";

    void Start()
    {
        initCode = File.ReadAllText(initFilePath);
        exeCode = File.ReadAllText(exeFilePath);
        LuaManager.lua.DoString(initCode);
    }    

    [SerializeField] string filePath = "";

    [ContextMenu("Run Lua Script From File")]
    public void ExeFromFile()
    {
        var code = File.ReadAllText(filePath);
        code = $"{defineStart}\n{code}\n{defineEnd}\n{exeCode}";
        print(code);

        try
        {
            LuaManager.lua.DoString(code);
        }
        catch (Exception ex)
        {
            print(ex);
        }        
    }
    
    public static IEnumerator SayCo()
    {
        print("hello");
        yield return new WaitForSeconds(1);
        print("world");
        yield break;
    }
}
