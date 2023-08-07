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
    /// 自身の情報を送信元に送り返す
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
    /// 新規接続者の情報を受け取り、全体に送信する
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sourceId"></param>
    private void RPCResponseGetUserList(Dictionary<string, object> data, string sourceId)
    {
        // 接続先の情報を全体に送信
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

        // 相手の情報を要求する (Chunkの取得)
        //GM.Msg("RequestUserData", connectedId);

        var chunkData = data["chunk"].ToString().GetDict<string, int>();
        var chunk = (chunkData["Item1"], chunkData["Item2"], chunkData["Item3"]);
        GM.Msg("UpdateChunk", sourceId, chunk);
    }
}
