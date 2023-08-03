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
    /// User����Ԃ�
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
    /// Chunk�̍X�V
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
    /// ���݂�Chunk���ύX���ꂽ�Ƃ��ɌĂяo�����
    /// �߂���Chunk�𑗐M�Ώۂɂ���
    /// </summary>
    /// <param name="currentChunk"></param>
    public void OnChangeChunk((int, int, int) currentChunk)
    {
        // ���M�Ώۂ�Chunk���X�V
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

        // Chunk�̕ύX�𑗐M
        sendData.ForceAdd("chunk", currentChunk);
        GM.Msg("RTCSendAll", sendData);
    }
}
