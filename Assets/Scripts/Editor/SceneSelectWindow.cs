using System.Linq;
using DC;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SceneSelectWindow : EditorWindow
{
    static DB_GameManager db;
    private Vector2 scrollPosition;

    [MenuItem("Tools/Scene Window")]
    public static void ShowWindow()
    {
        GetWindow<SceneSelectWindow>("Scene List");
        db = GM.mng;
    }

    private void OnGUI()
    {
        db ??= FindData();
        if (db == null)
        {
            return;
        }

        eScene.Scene firstScene = (eScene.Scene)EditorGUILayout.EnumPopup("", db.firstScene);
        if (firstScene != db.firstScene)
        {
            // ï€ë∂
            db.firstScene = firstScene;
            EditorUtility.SetDirty(db);
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (var group in db.data)
        {
            GUILayout.Label(group.groupName, EditorStyles.boldLabel);

            foreach (var scene in group.scene)
            {
                SetButton(scene);
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private static void SetButton(SceneAsset scene)
    {
        if (GUILayout.Button(scene.name))
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorApplication.isPlaying = false;
                string scenePath = AssetDatabase.GetAssetPath(scene);
                EditorSceneManager.OpenScene(scenePath);
            }
        }
    }

    DB_GameManager FindData()
    {
        Debug.Log("Load: DB_GameManager");
        var guids = AssetDatabase.FindAssets("t:DB_GameManager");

        // GUIDÇ©ÇÁAssetPathÇ…ïœä∑Ç∑ÇÈ
        var assetPaths = guids.Select(AssetDatabase.GUIDToAssetPath).ToArray();

        // AssetPathÇ©ÇÁëŒè€ÇÃScriptableObjectÇéÊìæÇ∑ÇÈ
        var scriptableObjects = assetPaths.Select(AssetDatabase.LoadAssetAtPath<DB_GameManager>).ToArray();
        return scriptableObjects.FirstOrDefault();
    }
}
