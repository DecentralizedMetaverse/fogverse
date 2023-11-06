using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class SearchWordWindow : EditorWindow
{
    private string searchString = "";
    private Dictionary<string, List<(int, string)>> searchResult = new();
    private Vector2 scrollPosition;

    [MenuItem("Tools/Search Window")]
    public static void ShowWindow()
    {
        GetWindow<SearchWordWindow>("Search Window");
    }

    private void OnGUI()
    {
        GUILayout.Label("Search in Project:", EditorStyles.boldLabel);

        searchString = EditorGUILayout.TextField("Search String:", searchString);

        if (GUILayout.Button("Search"))
        {
            // 検索処理を実行
            searchResult = SearchFiles($"{Application.dataPath}/Scripts/", searchString);
        }

        GUILayout.Space(10);

        GUILayout.Label("Search Result:");

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (var result in searchResult)
        {
            GUILayout.Label(Path.GetFileName(result.Key));
            foreach (var line in result.Value)
            {
                var (lineNum, lineStr) = line;
                if (GUILayout.Button(lineStr))
                {
                    InternalEditorUtility.OpenFileAtLineExternal(result.Key, lineNum + 1);
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }

    Dictionary<string, List<(int, string)>> SearchFiles(string directory, string searchString)
    {
        Dictionary<string, List<(int, string)>> result = new();
        try
        {
            // ディレクトリ内のすべてのファイルを取得
            string[] files = Directory.GetFiles(directory, "*.cs");

            // ファイルごとに検索
            foreach (string file in files)
            {
                var findLines = FileContainsString(file, searchString);
                if (findLines.Count == 0) continue;
                result.Add(file, findLines);
            }

            // サブディレクトリも再帰的に検索
            string[] subDirectories = Directory.GetDirectories(directory);
            foreach (string subDir in subDirectories)
            {
                var serachResult = SearchFiles(subDir, searchString);
                foreach (var item in serachResult)
                {
                    result.Add(item.Key, item.Value);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"An error occurred: {e.Message}");
        }

        return result;
    }

    List<(int, string)> FileContainsString(string filePath, string searchString)
    {
        var result = new List<(int, string)>();
        var lineNum = 0;
        foreach (var line in File.ReadLines(filePath))
        {
            if (line.Contains(searchString))
            {
                // 最初の空白を削除する
                var trimLine = line.TrimStart();
                result.Add((lineNum, trimLine));
            }
            lineNum++;
        }

        return result;
    }
}