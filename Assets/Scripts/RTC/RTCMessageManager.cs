using DC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WebRTC　Message送信
/// </summary>
public class RTCMessageManager : MonoBehaviour
{
    void Start()
    {
        GM.Add<Dictionary<string, object>>("RTCSendAll", (data) =>
        {
            foreach (var (id, peer) in GM.db.rtc.peers)
            {
                data.TryAdd("target_id", "*");
                data.TryAdd("id", GM.db.rtc.id);

                var dataTxt = data.GetString();
                peer.Send(dataTxt);
                GM.Msg("AddOutput", $"[Send] {dataTxt}");
            }

        });

        GM.Add<string, Dictionary<string, object>>("RTCSendDirect", (target_id, data) =>
        {
            if(GM.db.rtc.peers.TryGetValue(target_id, out var peer))
            {
                data.TryAdd("id", GM.db.rtc.id);
                data.TryAdd("target_id", target_id);
                
                var dataTxt = data.GetString();
                peer.Send(dataTxt);
                GM.Msg("AddOutput", $"[Send] ->{target_id} {dataTxt}");
            }
            else
            {
                GM.Msg("AddOutput", $"[沒有][Send] ->{target_id} {data.GetString()}");
            }
        });

        // TODO: ChunkとGroupで使用する
        GM.Add<string, string>("RTCSendGroup", (id, data) =>
        {

        });
    }
}
