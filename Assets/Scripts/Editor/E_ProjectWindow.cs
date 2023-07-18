using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class E_ProjectWindow : EditorWindow
{
    //[MenuItem("Tools/ProjectWindow")]
    //public static void Open()
    //{
    //    GetWindow<E_ProjectWindow>("Project2");
    //}
    //private static MethodInfo _getInstanceIDMethod;
    //static E_ProjectWindow()
    //{
    //    _getInstanceIDMethod = typeof(AssetDatabase).GetMethod("GetMainAssetInstanceID",
    //        BindingFlags.Static | BindingFlags.NonPublic);
    //}
    //private void OnGUI()
    //{
    //    string folderPath = "Assets/Scripts";
    //    var obj = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folderPath);

    //    var projectBrowserType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
    //    var projectBrowser = EditorWindow.GetWindow(projectBrowserType);

    //    var _ShowFolderContents = projectBrowserType.GetMethod
    //    (
    //        "ShowFolderContents",
    //        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
    //        null,
    //        new System.Type[] { typeof(int), typeof(bool) }, null
    //    );
    //    //_ShowFolderContents.Invoke(projectBrowser, new object[] { obj.GetInstanceID(), true });
    //    //using (var helper = EditorGUILayoutHelper.Horizontal(out var rect))
    //    EditorGUILayout.BeginHorizontal();
    //    ShowFolderContents((int)_getInstanceIDMethod.Invoke(null, new object[] { "Assets" }));
    //    EditorGUILayout.EndHorizontal();
    //}
    //private static void ShowFolderContents(int folderInstanceID)
    //{
    //    // Find the internal ProjectBrowser class in the editor assembly.
    //    Assembly editorAssembly = typeof(Editor).Assembly;
    //    System.Type projectBrowserType = editorAssembly.GetType("UnityEditor.ProjectBrowser");

    //    // This is the internal method, which performs the desired action.
    //    // Should only be called if the project window is in two column mode.
    //    MethodInfo showFolderContents = projectBrowserType.GetMethod(
    //        "ShowFolderContents", BindingFlags.Instance | BindingFlags.NonPublic);

    //    // Find any open project browser windows.
    //    Object[] projectBrowserInstances = Resources.FindObjectsOfTypeAll(projectBrowserType);

    //    if (projectBrowserInstances.Length > 0)
    //    {
    //        for (int i = 0; i < projectBrowserInstances.Length; i++)
    //            ShowFolderContentsInternal(projectBrowserInstances[i], showFolderContents, folderInstanceID);
    //    }
    //    else
    //    {
    //        EditorWindow projectBrowser = OpenNewProjectBrowser(projectBrowserType);
    //        ShowFolderContentsInternal(projectBrowser, showFolderContents, folderInstanceID);
    //    }
    //}
    //private static void ShowFolderContentsInternal(Object projectBrowser, MethodInfo showFolderContents, int folderInstanceID)
    //{
    //    // Sadly, there is no method to check for the view mode.
    //    // We can use the serialized object to find the private property.
    //    SerializedObject serializedObject = new SerializedObject(projectBrowser);
    //    bool inTwoColumnMode = serializedObject.FindProperty("m_ViewMode").enumValueIndex == 1;

    //    if (!inTwoColumnMode)
    //    {
    //        // If the browser is not in two column mode, we must set it to show the folder contents.
    //        MethodInfo setTwoColumns = projectBrowser.GetType().GetMethod(
    //            "SetTwoColumns", BindingFlags.Instance | BindingFlags.NonPublic);
    //        setTwoColumns.Invoke(projectBrowser, null);
    //    }

    //    bool revealAndFrameInFolderTree = true;
    //    showFolderContents.Invoke(projectBrowser, new object[] { folderInstanceID, revealAndFrameInFolderTree });
    //}
    //private static EditorWindow OpenNewProjectBrowser(System.Type projectBrowserType)
    //{
    //    EditorWindow projectBrowser = EditorWindow.GetWindow(projectBrowserType);
    //    projectBrowser.Show();

    //    // Unity does some special initialization logic, which we must call,
    //    // before we can use the ShowFolderContents method (else we get a NullReferenceException).
    //    MethodInfo init = projectBrowserType.GetMethod("Init", BindingFlags.Instance | BindingFlags.Public);
    //    init.Invoke(projectBrowser, null);

    //    return projectBrowser;
    //}
}