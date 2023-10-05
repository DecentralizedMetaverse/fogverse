using System.Collections.Generic;
using DC;
using MemoryPack;
using UnityEngine;

public class RPCManager : MonoBehaviour
{
    void Start()
    {
        GM.Add<RTCMessage, string, string>("RECV_RPC", (message, sourceId, relayId) =>
        {
            var data = MemoryPackSerializer.Deserialize<P_RPC>(message.data);
            var dataDict = data.args.GetDict<string, object>();
            dataDict.ForceAdd("relayId", relayId);
            GM.Msg($"RPC_{data.method}", dataDict, sourceId);
        });

        GM.Add<Dictionary<string, object>>("RPCSendAll", (data) =>
        {
            data.ForceAdd("targetId", "*");
            data.ForceAdd("id", GM.db.rtc.id);

            var message = new P_RPC
            {
                method = data["type"].ToString(),
                args = data.GetString(),
            };

            var bytes = MemoryPackSerializer.Serialize(message);
            GM.Msg("RTCSendAll", MessageType.RPC, bytes);
        });

        GM.Add<string, Dictionary<string, object>>("RPCSendDirect", (targetId, data) =>
        {
            data.ForceAdd("id", GM.db.rtc.id);
            data.ForceAdd("targetId", targetId);

            var message = new P_RPC
            {
                method = data["type"].ToString(),
                args = data.GetString(),
            };

            var bytes = MemoryPackSerializer.Serialize(message);
            GM.Msg("RTCSendDirect", MessageType.RPC, targetId, bytes);
        });
    }
}
