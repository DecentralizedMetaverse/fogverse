using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Log要求用のクラス
/// </summary>
public class RTCLog : MonoBehaviour
{
    /// <summary>
    /// 要求用のデータ
    /// </summary>
    Dictionary<string, object> sendData = new Dictionary<string, object>()
    {
        { "type", "log" } 
    };

    void Start()
    {
        GM.Add<Dictionary<string, object>, string>("RPC_log", RPCLog);
        GM.Add<Dictionary<string, object>, string>("RPC_response_log", RPCResponseLog);

        GM.Add("GetAllLog", () =>
        {
            // 全体ClientにLogを要求する
            GM.Msg("RTCSendAll", sendData);
        });
    }

    /// <summary>
    /// 送り主にLogを返す
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sourceId"></param>
    private void RPCLog(Dictionary<string, object> data, string sourceId)
    {
        var evaluationData = GM.Msg<string>("GetEvaluationData");
        var outputData = GM.Msg<string>("GetLog");

        Dictionary<string, object> sendData = new Dictionary<string, object>()
        {
            { "type", "response_log" },
            { "eval", evaluationData },
            { "output",outputData }
        };

        GM.Msg("RTCSendDirect", sourceId, sendData);
    }

    /// <summary>
    /// Logを保存する
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sourceId"></param>
    private void RPCResponseLog(Dictionary<string, object> data, string sourceId)
    {
        var eval = data["eval"].ToString();
        var path = $"{Application.dataPath}/{GM.mng.outputPath}/{DateTime.Now.ToString("yyyy-MM-dd-HH-mm")}-{sourceId}-eval.csv";
        System.IO.File.WriteAllText(path, eval);

        var output = data["output"].ToString();
        path = $"{Application.dataPath}/{GM.mng.outputPath}/{DateTime.Now.ToString("yyyy-MM-dd-HH-mm")}-{sourceId}-output.log";
        System.IO.File.WriteAllText(path, output);
    }
}
