using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DC;
using UnityEngine;
using UnityEngine.Analytics;

[CreateAssetMenu(fileName = "DB_FunctionList", menuName = "DB/DB_FunctionList")]
public class DB_FunctionList : ScriptableObject
{
    public List<DB_FunctionListE> data = new List<DB_FunctionListE>();

    /// <summary>
    /// �h�L�������g����
    /// </summary>
    /// <returns></returns>
    public string GetDocs()
    {
        var docsTxt = "";

        foreach (var dt in data)
        {
            docsTxt += $"## {dt.functionName}\n";

            // ����
            docsTxt += $"### Description\n{dt.description}\n";

            // ����
            if (dt.@params != "")
            {
                docsTxt += $"### Parameters\n";
                var splitParams = dt.@params.Split(',');
                foreach (var p in splitParams)
                {
                    if (p == "") continue;
                    docsTxt += $"- {p}\n";
                }
            }

            // �Ԃ�l
            if (dt.@return != "")
            {
                docsTxt += $"### Returns\n" +
                    $"- {dt.@return}\n";
            }
            docsTxt += "---\n";
        }

        return docsTxt;
    }

    /// <summary>
    /// �o�^���ꂽ�֐����ꗗ�Ƃ��ăe�L�X�gFile�ɏ����o��
    /// </summary>
    public void SaveFunctionList(Dictionary<string, List<Delegate>> functions)
    {
        data.Clear(); // ������

        // �o�^���ꂽ�֐��̎擾
        foreach (var functionName in functions.Keys)
        {
            var name = functionName;
            DB_FunctionListE data = new();
            data.functionName = name;

            // �����擾                
            var paramsTxt = GetParamsTxt(name, functions);
            data.@params = paramsTxt;

            // �Ԃ�l�擾
            var returnTxt = functions[name][0].GetMethodInfo().ReturnType.ToString();
            data.@return = returnTxt;

            this.data.Add(data);
        }
    }

    /// <summary>
    /// �����̎擾
    /// </summary>
    /// <param name="name">functions��key</param>
    /// <returns></returns>
    private string GetParamsTxt(string name, Dictionary<string, List<Delegate>> functions)
    {
        var param = functions[name][0].GetMethodInfo().GetParameters();
        var parmTxt = "";
        foreach (var p in param)
        {
            var type = p.ParameterType.ToString();
            var parmName = p.Name;
            parmTxt += $"{type} {parmName},";
        }

        return parmTxt;
    }
}

[System.Serializable]
public class DB_FunctionListE
{
    public string functionName;
    public string description;
    public string @params;
    public string @return;
}
