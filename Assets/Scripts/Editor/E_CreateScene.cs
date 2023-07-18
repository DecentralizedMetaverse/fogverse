using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;

public class E_CreateScene
{
    [MenuItem("Assets/Create/Create Scene", priority = 51)]
    static void CreateScene()
    {
        var path = EditorUtility.SaveFilePanel("Scene", "Assets/Scenes", "", "unity");

        if (string.IsNullOrEmpty(path)) return;

        path = FileUtil.GetProjectRelativePath(path);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, path);

        var scenes = EditorBuildSettings.scenes;
        ArrayUtility.Add(ref scenes,new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes;
    }
}
