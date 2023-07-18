using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DB_GameManager))]
public class E_GameManager : Editor
{
    const string enumPath = "Assets/Scripts/Core/Enum/eScene.cs";
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Apply"))
        {
            var list = target as DB_GameManager;
            GenerateEnumFile(list);
        }
    }

    /// <summary>
    /// Enum生成
    /// </summary>
    /// <param name="list"></param>
    private static void GenerateEnumFile(DB_GameManager list)
    {
        List<string> str = new List<string>();
        for (var i = 0; i < list.data.Count; i++)
        {
            str.Add(list.data[i].groupName);
        }
        EnumManager.Create("Scene", enumPath, str);
        AssetDatabase.Refresh();
    }
}
