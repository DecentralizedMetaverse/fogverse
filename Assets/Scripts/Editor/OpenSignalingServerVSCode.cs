using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class OpenSignalingServerVSCode
{
    [MenuItem("Tools/Open SignalingServer")]
    static void Execute()
    {
        Process.Start("code", $"{Application.dataPath}/../SignalingServer");
    }

    [MenuItem("Tools/Run SignalingServer")]
    static void RunServer()
    {
        string serverPath = $"{Application.dataPath}/../SignalingServer/server/";
        Process.Start("powershell.exe", $"-Command \"cd {serverPath};go run server.go; Read-Host 'Press Enter to exit...'\"");
    }
}
