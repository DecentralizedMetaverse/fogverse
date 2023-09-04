using DC;
using MemoryPack;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
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
            GM.Msg($"RPC_{data.method}", dataDict);
        });

        GM.Add<Dictionary<string, object>>("RPCSendAll", (data) =>
        {
            foreach (var (id, peer) in GM.db.rtc.peers)
            {
                data.ForceAdd("targetId", "*");
                data.ForceAdd("id", GM.db.rtc.id);

                var message = new P_RPC
                {
                    method = data["type"].ToString(),
                    args = data.GetString(),
                };

                peer.Send(MemoryPackSerializer.Serialize(message));
            }
        });

        GM.Add<string, Dictionary<string, object>>("RPCSendDirect", (targetId, data) =>
        {
            if (!GM.db.rtc.peers.TryGetValue(targetId, out var peer)) return;
            data.ForceAdd("id", GM.db.rtc.id);
            data.ForceAdd("targetId", targetId);

            var message = new P_RPC
            {
                method = data["type"].ToString(),
                args = data.GetString(),
            };

            peer.Send(MemoryPackSerializer.Serialize(message));
        });
    }
}
