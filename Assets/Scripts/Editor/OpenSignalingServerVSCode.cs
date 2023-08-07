using System.Collections;
using System.Collections.Generic;
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
        Process.Start("go",$"run {Application.dataPath}/../SignalingServer/server.go");
    }
}
