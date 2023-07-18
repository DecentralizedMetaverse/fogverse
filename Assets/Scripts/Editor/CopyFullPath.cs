using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CopyFullPath
{
    [MenuItem("Assets/Copy Full Path", priority = 19)]
    static void Execute()
    {
        // get select GO full path
        int instanceID = Selection.activeInstanceID;
        string path = AssetDatabase.GetAssetPath(instanceID);
        string fullPath = System.IO.Path.GetFullPath(path);


        // copy clipboard
        GUIUtility.systemCopyBuffer = fullPath;
        Debug.Log("Copy clipboard : \n" + fullPath);
    }
}
