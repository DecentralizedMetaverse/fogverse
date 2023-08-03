using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    Dictionary<string, object> sendData = new()
    {
        { "type", "chunk" },
    };

    private void Start()
    {
        GM.Add<Dictionary<string, object>, string>("RPC_chunk", RPCChunk);
        GM.Add<Dictionary<string, object>, string>("RPC_user_data", RPCChunk);
        GM.Add<(int, int, int)>("ChangeChunk", OnChangeChunk);
        GM.Add<Dictionary<string, object>, string>("RPC_request_user_data", RPC_RequestUserData);
        GM.Add<string>("RequestUserData", RequestUserData);

    }
    private void RequestUserData(string targetId)
    {
        Dictionary<string, object> sendData = new()
        {
            { "type", "reqest_user_data" },
        };

        GM.Msg("RTCSendDirect", targetId, sendData);
    }

    /// <summary>
    /// User情報を返す
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sourceId"></param>
    private void RPC_RequestUserData(Dictionary<string, object> data, string sourceId)
    {
        Dictionary<string, object> sendData = new()
        {
            { "type", "user_data" },
            { "chunk", GM.db.player.chunk },
        };

        GM.Msg("RTCSendDirect", sourceId, sendData);
    }

    /// <summary>
    /// Chunkの更新
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sourceId"></param>
    private void RPCChunk(Dictionary<string, object> data, string sourceId)
    {
        var chunkData = data["chunk"].ToString().GetDict<string,int>();
        var chunk = (chunkData["Item1"], chunkData["Item2"], chunkData["Item3"]);
        GM.Msg("AddChunk", sourceId, chunk);
    }

    /// <summary>
    /// 現在のChunkが変更されたときに呼び出される
    /// 近くのChunkを送信対象にする
    /// </summary>
    /// <param name="currentChunk"></param>
    public void OnChangeChunk((int, int, int) currentChunk)
    {
        // 送信対象のChunkを更新
        GM.db.chunk.sendTargetChunk.Clear();
        var (x, y, z) = currentChunk;

        for (var i = -1; i <= 1; i++)
        {
            for (var j = -1; j <= 1; j++)
            {
                for (var k = -1; k <= 1; k++)
                {
                    GM.db.chunk.sendTargetChunk.Add((x + i, y + j, z + k));
                }
            }
        }

        // Chunkの変更を送信
        sendData.ForceAdd("chunk", currentChunk);
        GM.Msg("RTCSendAll", sendData);
    }
}
