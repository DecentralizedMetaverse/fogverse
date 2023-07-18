using DC;
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
        path = $"{Application.dataPath}/{path}";
    }

    public void OnClick()
    {
        File.WriteAllText(path, GM.Msg<string>("GetLog"));
    }
}
