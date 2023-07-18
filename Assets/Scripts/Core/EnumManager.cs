#pragma warning disable CS0649

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
/// <summary>
/// Enumを自動で定義する
/// </summary>
public class EnumManager : MonoBehaviour
{
    [SerializeField] DB_Scene data;
    public bool updateSceneEnum;
    [SerializeField] string path;
    private void Start()
    {
#if UNITY_EDITOR
#elif UNITY_STANDALONE
    return;
#endif        
        if (!updateSceneEnum) return;

        List<string> str = new List<string>();

        for (var i = 0; i < data.data.Count; i++)
        {
            str.Add(data.data[i].name);
        }
        Create("Scene", path, str);
    }
    public static void Create(string enumName, string path, List<string> data)
    {
#if UNITY_EDITOR
#elif UNITY_STANDALONE
    return;
#endif
        string target = "";

        //Fileが存在しない
        if (!File.Exists(path))
        {
            New(enumName, path, data);
            return;
        }
        target = File.ReadAllText(path).Replace("\r", string.Empty);
        int start = target.IndexOf('{');
        string str = target.Substring(0, start + 2);

        str += "    public enum " + enumName + " { ";

        for (var i = 0; i < data.Count; i++)
        {
            str += data[i] + ", ";
        }
        str += "}\n}";
        File.WriteAllText(path, str, System.Text.Encoding.UTF8);
    }
    static void New(string enumName, string path, List<string> data)
    {
#if UNITY_EDITOR
#elif UNITY_STANDALONE
    return;
#endif
        if (!File.Exists(path)) File.Create(path).Close();
        string str =
           "public class " + Path.GetFileName(path).Replace(".cs", "") +
           "\n{\n    public enum " + enumName + " { ";

        for (var i = 0; i < data.Count; i++)
        {
            str += data[i] + ", ";
        }
        str += "}\n}";
        File.WriteAllText(path, str, System.Text.Encoding.UTF8);
    }
}
