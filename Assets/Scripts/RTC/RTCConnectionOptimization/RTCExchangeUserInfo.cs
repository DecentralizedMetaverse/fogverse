using DC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTCExchangeUserInfo : MonoBehaviour
{
    void Start()
    {
        GM.Add<Dictionary<string, object>, string>("RPC_get_user_list", RPCGetUserList);
        GM.Add<Dictionary<string, object>, string>("RPC_response_get_user_list", RPCResponseGetUserList);
    }

    /// <summary>
    /// ���g�̏��𑗐M���ɑ���Ԃ�
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="sourceId"></param>
    private void RPCGetUserList(Dictionary<string, object> dictionary, string sourceId)
    {
        List<string> ids = new List<string>(GM.db.rtc.peers.Keys);
        ids.Add(GM.db.rtc.id);

        Dictionary<string, object> sendData = new()
        {
            {"type", "response_get_user_list"},
            {"ids", ids},
            {"chunk", GM.db.player.chunk}
        };
        
        GM.Msg("RTCSendDirect", sourceId, sendData);
    }

    /// <summary>
    /// �V�K�ڑ��҂̏����󂯎��A�S�̂ɑ��M����
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sourceId"></param>
    private void RPCResponseGetUserList(Dictionary<string, object> data, string sourceId)
    {
        // �ڑ���̏���S�̂ɑ��M
        Dictionary<string, object> sendData = new()
        {
            { "type", "join" },
            { "join_ids", data["ids"] },
            { "chunk", data["chunk"] }
        };        

        foreach (var (id, peer) in GM.db.rtc.peers)
        {
            if (id == sourceId) continue;
            GM.Msg("RTCSendDirect", id, sendData);
        }

        // ����̏���v������ (Chunk�̎擾)
        //GM.Msg("RequestUserData", connectedId);

        var chunkData = data["chunk"].ToString().GetDict<string, int>();
        var chunk = (chunkData["Item1"], chunkData["Item2"], chunkData["Item3"]);
        GM.Msg("UpdateChunk", sourceId, chunk);
    }
}
