using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Log�v���p�̃N���X
/// </summary>
public class RTCLog : MonoBehaviour
{
    /// <summary>
    /// �v���p�̃f�[�^
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
            // �S��Client��Log��v������
            GM.Msg("RTCSendAll", sendData);
        });
    }

    /// <summary>
    /// ������Log��Ԃ�
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
    /// Log��ۑ�����
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
