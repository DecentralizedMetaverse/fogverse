using System.Collections.Generic;
using DC;
using UnityEngine;

public class ChatModel : MonoBehaviour
{
    Dictionary<string, object> sendData = new();
    void Start()
    {
        sendData["type"] = "message";
        sendData["content"] = "";

        GM.Add<string, string>("SendChat", (id, txt) =>
        {
            sendData["content"] = txt;
            GM.Msg("RPCSendAll", sendData);
        });

        GM.Add<Dictionary<string, object>, string>("RPC_message", (data, sourceId) =>
        {
            GM.Msg("AddChatMessage", sourceId, data["content"].ToString());
        });
    }


}
