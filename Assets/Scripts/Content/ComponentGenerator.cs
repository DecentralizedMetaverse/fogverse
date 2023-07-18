using Cysharp.Threading.Tasks;
using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Component�𐶐���Object�ɒǉ�����
/// </summary>
public class ComponentGenerator : MonoBehaviour
{
    Dictionary<string, Delegate> functions = new(64);

    void Start()
    {
        GM.Add<string, Transform, Dictionary<string, object>>("GenerateComponent", GenerateComponent);
        GM.Add("GetComponentExtensions", GetComponentExtensions);
        AddFunc<string, Transform, Dictionary<string, object>>(".lua", ComponentLua);        
    }

    /// <summary>
    /// Component����
    /// </summary>
    /// <param name="path"></param>
    /// <param name="root"></param>
    void GenerateComponent(string path, Transform root, Dictionary<string, object> custom)
    {
        var extension = System.IO.Path.GetExtension(path);
        functions[extension].DynamicInvoke(path, root, custom);
    }

    /// <summary>
    /// �Ή�����g���q�̃��X�g��Ԃ�
    /// </summary>
    /// <returns></returns>
    string[] GetComponentExtensions()
    {
        return functions.Keys.ToArray();
    }

    /// <summary>
    /// �֐���o�^����
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="func"></param>
    void AddFunc<T1, T2, T3>(string key, Action<T1, T2, T3> func)
    {
        functions.Add(key, func);
    }

    // ------------------------------------------------
    void ComponentLua(string path, Transform root, Dictionary<string, object> custom)
    {
        var txt = File.ReadAllText(path);        
        var eventObj = root.gameObject.AddComponent<EventObject3D>();
        eventObj.code = txt;
        eventObj.custom = custom;
        eventObj.type  = (eYScript.exeType)Enum.Parse(typeof(eYScript.exeType), custom["run"].ToString());
        eventObj.fileName = path;
    }    
}
