using GluonGui.WorkspaceWindow.Views.WorkspaceExplorer;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// �o�^���ꂽ�֐����h�L�������g������
/// </summary>
[CustomEditor(typeof(DB_FunctionList))]
public class FunctionsInfoToDocs : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!GUILayout.Button("Copy Docs")) return;

        var data = target as DB_FunctionList;
        GUIUtility.systemCopyBuffer = data.GetDocs();
    }
}
