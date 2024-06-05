using System.Collections.Generic;
using UnityEngine;
using B83.Win32;
using DC;
using Cysharp.Threading.Tasks;
using System.IO;

public class FileDragDrop : MonoBehaviour
{
    private readonly string[][] _select =
    {
        new[] { "Upload", "Cancel", "UploadAll" },
        new[] { "Upload", "Cancel" }
    };

    private void Start()
    {
        UnityDragAndDropHook.InstallHook();
        UnityDragAndDropHook.OnDroppedFiles += OnFiles;
    }

    private void OnDisable()
    {
        UnityDragAndDropHook.UninstallHook();
    }

    private async void OnFiles(List<string> aPathNames, POINT aDropPoint)
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
                result = await GM.Msg<UniTask<int>>("Question", (object)_select[0]);
            }
            else
            {
                result = await GM.Msg<UniTask<int>>("Question", (object)_select[1]);
            }

            GM.Msg("QuestionTitle", "");

            // Cancel
            if (result == 1)
            {
                continue;
            }

            // Upload
            Upload(contentPath);
        }
    }

    private static void Upload(string path)
    {
        GM.Msg("ShortMessage", path);
        GM.Msg("GenerateObj", path);
        GM.Msg("RegisterObject");
    }
}
