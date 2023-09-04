using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using YamlDotNet.Serialization;


public class ProtocolClassGenerator : MonoBehaviour
{
    const string enumFileName = "MessageType";
    const string parentPath = "Assets/Scripts/Core/Network/Protocol";
    const string yamlFileName = "protocol.yaml";


    [MenuItem("Tools/Generate Protocol Classes")]
    static void GenerateClassesFromYAML()
    {
        var data = Read($"{parentPath}/{yamlFileName}");
        GenerateClasses(data);
        AssetDatabase.Refresh();
    }

    static void GenerateClasses(Dictionary<string, object> data)
    {
        var types = new List<string>();
        foreach (var message in data["messages"] as List<object>)
        {
            var messageInfo = (IDictionary)message;
            var className = messageInfo["name"].ToString();
            var fields = messageInfo["fields"] as List<object>;

            types.Add(className);
            GenerateClass(className, fields);
        }

        GenerateEnumFile(types);
    }

    static void GenerateEnumFile(List<string> types)
    {
        var txt = $"public enum {enumFileName}" +
            "\n{\n";
        foreach(var type in types)
        {
            txt += $"    {type},\n";
        }
        txt += "}\n";
        File.WriteAllText($"{parentPath}/{enumFileName}.cs", txt);
    }

    static void GenerateClass(string className, List<object> fields)
    {
        className = $"P_{className}";

        var generatedCode = $"using MemoryPack;\n" +
            $"using UnityEngine;\n\n" +
            $"[MemoryPackable]\n" +
            $"public partial class {className}\n";
        generatedCode += "{\n";

        foreach (var field in fields)
        {
            var fieldInfo = (IDictionary)field;
            var fieldName = fieldInfo["name"] as string;
            var fieldType = fieldInfo["type"] as string;

            generatedCode += $"    public {GetCSharpType(fieldType)} {fieldName} {{ get; set; }}\n";
        }

        generatedCode += "}\n";

        File.WriteAllText($"{parentPath}/{className}.cs", generatedCode);
    }

    static string GetCSharpType(string fieldType)
    {
        var typeMapping = new Dictionary<string, string>
        {
            {"string", "string"},
            {"int", "int"},
            {"float", "float"},
            {"boolean", "bool"},
            {"Vector3", "Vector3"},
        };

        if (typeMapping.ContainsKey(fieldType))
        {
            return typeMapping[fieldType];
        }

        return "object";
    }

    static Dictionary<string, object> Read(string path)
    {
        if (!File.Exists(path)) return null;

        var input = File.ReadAllText(path);
        var result = ReadText(input);

        return result;
    }

    static Dictionary<string, object> ReadText(string input)
    {
        var deserializer = new DeserializerBuilder().Build();
        var result = deserializer.Deserialize<Dictionary<string, object>>(input);

        return result;
    }
}