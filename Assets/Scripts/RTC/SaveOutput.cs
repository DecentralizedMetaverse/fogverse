using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class SaveOutput : MonoBehaviour
{
    string path = "output.log";

    private void Start()
    {
        path = $"{Application.dataPath}/{GM.mng.outputPath}/{DateTime.Now.ToString("yyyy-MM-dd-HH-mm")}-{path}";
    }

    public void OnClick()
    {
        File.WriteAllText(path, GM.Msg<string>("GetLog"));
    }

    private void OnDestroy()
    {
        File.WriteAllText(path, GM.Msg<string>("GetLog"));
    }
}
