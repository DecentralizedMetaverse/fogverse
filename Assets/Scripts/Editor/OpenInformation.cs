using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class OpenInformation
{
    [MenuItem("Tools/Open Config")]
    static void Execute()
    {
        Process.Start("code", $"{Application.dataPath}/../config.json");
    }
}
