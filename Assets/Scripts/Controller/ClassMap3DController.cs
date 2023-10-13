using System.Collections.Generic;
using DC;
using UnityEngine;

public class ClassMap3DController : MonoBehaviour
{
    [SerializeField] ClassMap3DElement prefab;
    private List<DB_FunctionListE> functionData;
    Dictionary<string, ClassMap3DElement> functionObjs = new();

    void Start()
    {
        functionObjs.Clear();
        GM.Add("Generate3DMap", Generate3DMap);
        GM.Add<string, string>("DebugExecuteFunction", DebugExecuteFunction);
        functionData = GM.db.functionList.data;
    }

    void DebugExecuteFunction(string from, string to)
    {

    }

    private void Generate3DMap()
    {
        for (var i = 0; i < functionData.Count; i++)
        {
            var pos = transform.position;
            pos.z = i * 2;

            var obj = Instantiate(prefab, pos, Quaternion.identity, transform);
            var function = functionData[i];
            obj.SetData(function.functionName, function.description);
            functionObjs.Add(function.functionName, obj);
        }
    }
}
