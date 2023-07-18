using System.Collections.Generic;
using UnityEngine;
using XLua;
using System;
using Cysharp.Threading.Tasks;
using DC;
using System.IO;

/// <summary>
/// Luaを管理するクラス
/// </summary>
public class LuaManager : MonoBehaviour
{
    const string defineStart = "local co = coroutine.create(function()";
    const string defineEnd = "end)";

    public static LuaEnv lua { get; private set; }
    public static LuaEnv luaInternal { get; private set; }

    [SerializeField] TextAsset initCode;
    [SerializeField] TextAsset exeCode;
    
    int runId = 0;
    Dictionary<int, GameObject> runTable = new(100);

    void Awake()
    {
        lua = new LuaEnv();
        luaInternal = new LuaEnv();
        RegisterGetFunction();

        luaInternal.DoString(initCode.text);
        lua.DoString(initCode.text);
    }

    private void Start()
    {
        GM.Add<string, GameObject, UniTask>("Run", Run, true);
        GM.Add<int>("ExitScript", ExitScript, true);

        // Luaが実行されているObjectを取得する
        GM.Add<int, GameObject>("GetObject", (runId) =>
        {
            if (runTable.TryGetValue(runId, out GameObject target))
            {
                return target;
            }

            return null;
        }, true);
    }

    private void OnDestroy()
    {
        lua.Dispose();
        luaInternal.Dispose();
    }

    /// <summary>
    /// C#の関数を登録する際に使用する関数を登録する
    /// </summary>
    private void RegisterGetFunction()
    {
        lua.DoString($"Log = CS.UnityEngine.Debug.Log");
        luaInternal.DoString($"Log = CS.UnityEngine.Debug.Log");

        lua.DoString($"GetFunction = CS.DC.GM.GetFunction");
        luaInternal.DoString($"GetFunction = CS.DC.GM.GetFunction");

        lua.AddLoader(CustomLoader);
        luaInternal.AddLoader(CustomLoader);
    }

    /// <summary>
    /// LuaFileを読み込めるようにする
    /// </summary>
    /// <param name="filepath"></param>
    /// <returns></returns>
    private byte[] CustomLoader(ref string filepath)
    {
        if (File.Exists(filepath))
        {
            return File.ReadAllBytes(filepath);
        }

        return null;    // ローダ読み込み失敗
    }      

    /// <summary>
    /// LuaにFunctionを登録する
    /// </summary>
    /// <param name="key"></param>
    /// <param name="function"></param>
    public static void RegisterLuaFunction(LuaEnv lua, string key, Delegate function)
    {
        lua.DoString($"{key} = GetFunction(\"{key}\");");
    }

    /// <summary>
    /// Lua Scriptを実行する
    /// </summary>
    /// <param name="code"></param>
    async UniTask Run(string code, GameObject target)
    {
        GM.pause = ePause.mode.GameStop;
        int runId = StartScript(target);
        
        // gameObject.AddComponent<Rigidbody>();

        code = $"{defineStart}\n" +
            $"local runId = {runId}\n" +
            $"{code}\n" +
            $"ExitScript(runId)\n" +
            $"{defineEnd}\n" +
            $"{exeCode.text}";

        GM.Log("Lua実行");
        
        code = code.Replace("return", "ExitScript(runId)\nreturn");

        try
        {
            LuaManager.lua.DoString(code);
        }
        catch (Exception ex)
        {
            print(ex);
        }

        await UniTask.WaitWhile(() => (runTable.ContainsKey(runId)));
        GM.pause = ePause.mode.none;
    }

    /// <summary>
    /// 実行されたLuaのObjectを登録する
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    int StartScript(GameObject target)
    {
        runTable.Add(runId, target);
        return runId++;
    }

    /// <summary>
    /// 実行終了したObjectを削除する
    /// </summary>
    /// <param name="runId"></param>
    void ExitScript(int runId)
    {
        runTable.Remove(runId);
    }
}