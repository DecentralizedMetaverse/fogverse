using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using B83.Win32;
using DC;
using Cysharp.Threading.Tasks;
using System.IO;

public class FileDragDrop : MonoBehaviour
{
    string[][] select = new string[][]
    {
        new string[]{"Upload", "Cancel", "UploadAll" },
        new string[]{"Upload", "Cancel" }
    };

    void Start()
    {        
        UnityDragAndDropHook.InstallHook();
        UnityDragAndDropHook.OnDroppedFiles += OnFiles;
    }

    void OnDisable()
    {
        UnityDragAndDropHook.UninstallHook();
    }

    async void OnFiles(List<string> aPathNames, POINT aDropPoint)
    {
        var result = 0;
        foreach (var contentPath in aPathNames)
        {
            if (result == 2)
            {
                // UploadAll
                Upload(contentPath);
                continue;
            }

            // Question
            GM.Msg("QuestionTitle", Path.GetFileName(contentPath));
            if (aPathNames.Count > 1)
            {
                result = await GM.Msg<UniTask<int>>("Question", (object)select[0]);
            }
            else
            {
                result = await GM.Msg<UniTask<int>>("Question", (object)select[1]);
            }

            GM.Msg("QuestionTitle", "");

            // Cancel
            if (result == 1) { continue; }

            // Upload
            Upload(contentPath);
        }
    }

    void Upload(string path)
    {
        GM.Msg("ShortMessage", path);
        GM.Msg("GenerateObj", path);
        GM.Msg("RegisterObject");
    }
}
