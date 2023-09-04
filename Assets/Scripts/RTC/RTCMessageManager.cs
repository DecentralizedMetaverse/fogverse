using DC;
using MemoryPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WebRTC�@Message���M
/// </summary>
public class RTCMessageManager : MonoBehaviour
{
    void Start()
    {
        GM.Add<MessageType, byte[]>("RTCSendAll", (type, data) =>
        {
            foreach (var (id, peer) in GM.db.rtc.peers)
            {
                var message = new RTCMessage
                {
                    data = data, 
                    targetId = "*" ,
                    id = GM.db.rtc.id,
                    type = type,
                };                

                peer.Send(MemoryPackSerializer.Serialize(message));
            }

        });

        GM.Add<MessageType, string, byte[]>("RTCSendDirect", (type, targetId, data) =>
        {
            if(GM.db.rtc.peers.TryGetValue(targetId, out var peer))
            {
                var message = new RTCMessage
                {
                    data = data,
                    targetId = targetId,
                    id = GM.db.rtc.id,
                    type = type,
                };

                peer.Send(MemoryPackSerializer.Serialize(message));
            }
            else
            {
                // GM.Msg("AddOutput", $"[���L][Send] ->{target_id} {data.GetString()}");
            }
        });

        GM.Add<string>("RTCClose", (id) =>
        {
            GM.db.rtc.peers[id].Close();
        });
        
        //GM.Add<Dictionary<string, object>>("RTCSendAll", (data) =>
        //{
        //    foreach (var (id, peer) in GM.db.rtc.peers)
        //    {
        //        data.ForceAdd("target_id", "*");
        //        data.ForceAdd("id", GM.db.rtc.id);

        //        peer.Send(data);

        //        // GM.Msg("AddOutput", $"[Send] {dataTxt}");
        //    }

        //});

        //GM.Add<string, Dictionary<string, object>>("RTCSendDirect", (target_id, data) =>
        //{
        //    if(GM.db.rtc.peers.TryGetValue(target_id, out var peer))
        //    {
        //        data.ForceAdd("id", GM.db.rtc.id);
        //        data.ForceAdd("target_id", target_id);
                
        //        // var dataTxt = data.GetString();
        //        peer.Send(data);
        //        // GM.Msg("AddOutput", $"[Send] ->{target_id} {dataTxt}");
        //    }
        //    else
        //    {
        //        // GM.Msg("AddOutput", $"[���L][Send] ->{target_id} {data.GetString()}");
        //    }
        //});        
    }
}

