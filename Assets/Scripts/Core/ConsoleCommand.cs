using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DC;
using UnityEngine;
using System.Threading.Tasks;
using static eYScript;

/// <summary>
/// 
/// </summary>
public class ConsoleCommand : MonoBehaviour
{
    const string appName = "cmd.exe";
    void Start()
    {
        GM.Add<string, UniTask<string>>("CMD", CMD);
        GM.Add<string, string, UniTask<string>>("Exe", Exe);
    }

    async UniTask<string> CMD(string args)
    {
        var result = await Exe(appName, @$"/c {args} /w");
        return result;
    }

    async UniTask<string> Exe(string command, string args)
    {
        GM.Log($"{command} {args}");
        var psInfo = new ProcessStartInfo();
        psInfo.FileName = command;
        psInfo.Arguments = args;
        psInfo.UseShellExecute = false;
        psInfo.RedirectStandardOutput = true;
        psInfo.CreateNoWindow = true;
        var process = Process.Start(psInfo);

        //await UniTask.SwitchToThreadPool();
        //process.WaitForExit();
        //await UniTask.SwitchToMainThread();

        var result = await process.StandardOutput.ReadToEndAsync().AsUniTask();
        return result;
    }
}
