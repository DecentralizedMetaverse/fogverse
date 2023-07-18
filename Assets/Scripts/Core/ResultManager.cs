using DC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultManager : MonoBehaviour
{
    int num = 0;
    Dictionary<int, object[]> resultBuffer = new(256);

    void Start()
    {
        GM.Add<int>("AddResult", () =>
        {
            resultBuffer.Add(num, null);
            return num++;
        });

        GM.Add<int, object[]>("SetResult", (id, result) =>
        {
            resultBuffer[id] = result;
        });

        GM.Add<int, object[]>("GetResult", (id) =>
        {
            var result = resultBuffer[id];
            resultBuffer.Remove(id);

            return result;
        });
    }
}
