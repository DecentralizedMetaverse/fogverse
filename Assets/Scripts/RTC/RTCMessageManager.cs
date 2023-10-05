using DC;
using MemoryPack;
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
                    targetId = "*",
                    id = GM.db.rtc.id,
                    type = type,
                };

                peer.Send(MemoryPackSerializer.Serialize(message));
            }

        });

        GM.Add<MessageType, string, byte[]>("RTCSendDirect", (type, targetId, data) =>
        {
            if (GM.db.rtc.peers.TryGetValue(targetId, out var peer))
            {
                var message = new RTCMessage
                {
                    type = type,
                    id = GM.db.rtc.id,
                    targetId = targetId,
                    data = data,
                };

                var bytes = MemoryPackSerializer.Serialize(message);
                //var txt = System.Text.Encoding.UTF8.GetString(data);
                //Debug.Log($"■{type}\n■{txt}\n{message}\n■");
                // var re = MemoryPackSerializer.Deserialize<RTCMessage>(bytes);
                peer.Send(bytes);
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
    }
}

