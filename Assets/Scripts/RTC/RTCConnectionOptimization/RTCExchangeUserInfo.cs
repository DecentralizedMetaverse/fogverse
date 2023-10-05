using System.Collections.Generic;
using System.Text.RegularExpressions;
using DC;
using MemoryPack;
using UnityEngine;

public class RTCExchangeUserInfo : MonoBehaviour
{
    void Start()
    {
        GM.Add<RTCMessage, string, string>($"RECV_{nameof(MessageType.GetUserList)}", RPCGetUserList);
        GM.Add<RTCMessage, string, string>($"RECV_{nameof(MessageType.GetUserListResponse)}", RPCResponseGetUserList);
    }

    /// <summary>
    /// 自身の情報を送信元に送り返す
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="sourceId"></param>
    private void RPCGetUserList(RTCMessage data, string sourceId, string _)
    {
        // var getUserListData = MemoryPackSerializer.Deserialize<P_GetUserList>(data.data);


        List<string> ids = new List<string>(GM.db.rtc.peers.Keys);
        // P_UserDataResponse
        ids.Add(GM.db.rtc.id);

        P_GetUserListResponse response = new()
        {
            // ids = string.Join(",", ids),
            ids = ids,
            chunk = GM.db.player.chunk.ToString()
        };
        var bytes = MemoryPackSerializer.Serialize(response);
        GM.Msg("RTCSendDirect", MessageType.GetUserListResponse, sourceId, bytes);
    }

    /// <summary>
    /// 新規接続者の情報を受け取り、全体に送信する
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sourceId"></param>
    private void RPCResponseGetUserList(RTCMessage data, string sourceId, string _)
    {
        var getUserListData = MemoryPackSerializer.Deserialize<P_GetUserListResponse>(data.data);

        // 接続先の情報を全体に送信
        P_Join joinData = new P_Join()
        {
            joinIds = getUserListData.ids,
            chunk = getUserListData.chunk
        };
        var bytes = MemoryPackSerializer.Serialize(joinData);

        // TODO: 毎回全員送るのは冗長　まとめて送るのでも良いかも　
        foreach (var (id, peer) in GM.db.rtc.peers)
        {
            if (id == sourceId) continue;
            GM.Msg("RTCSendDirect", MessageType.Join, id, bytes);
        }

        // 相手の情報を要求する (Chunkの取得)
        //GM.Msg("RequestUserData", connectedId);

        MatchCollection matches = Regex.Matches(getUserListData.chunk, @"\d+");

        int first = int.Parse(matches[0].Value);
        int second = int.Parse(matches[1].Value);
        int third = int.Parse(matches[2].Value);

        var chunk = (first, second, third);
        GM.Msg("UpdateChunk", sourceId, chunk);
    }
}
