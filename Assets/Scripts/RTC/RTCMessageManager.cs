using DC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WebRTCÅ@MessageëóêM
/// </summary>
public class RTCMessageManager : MonoBehaviour
{
    void Start()
    {
        GM.Add<Dictionary<string, object>>("RTCSendAll", (data) =>
        {
            foreach (var (id, peer) in GM.db.rtc.peers)
            {
                data.ForceAdd("target_id", "*");
                data.ForceAdd("id", GM.db.rtc.id);

                var dataTxt = data.GetString();
                peer.Send(dataTxt);
                GM.Msg("AddOutput", $"[Send] {dataTxt}");
            }

        });

        GM.Add<string, Dictionary<string, object>>("RTCSendDirect", (target_id, data) =>
        {
            if(GM.db.rtc.peers.TryGetValue(target_id, out var peer))
            {
                data.ForceAdd("id", GM.db.rtc.id);
                data.ForceAdd("target_id", target_id);
                
                var dataTxt = data.GetString();
                peer.Send(dataTxt);
                GM.Msg("AddOutput", $"[Send] ->{target_id} {dataTxt}");
            }
            else
            {
                GM.Msg("AddOutput", $"[üìóL][Send] ->{target_id} {data.GetString()}");
            }
        });

        GM.Add<string>("RTCClose", (id) =>
        {
            GM.db.rtc.peers[id].Close();
        });
    }
}
