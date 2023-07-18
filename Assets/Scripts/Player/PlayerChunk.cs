using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using DC;
using UnityEngine;

/// <summary>
/// プレイヤーのチャンクを常時調べる
/// 変化時はWorldの読み込みを行う
/// TODO: UniTaskのCancel処理を行う
/// </summary>
public class PlayerChunk : MonoBehaviour
{
    [SerializeField] DB_Player dbPlayer;
    float divideChunkSize;

    void Start()
    {
        dbPlayer.user = transform;
        if(dbPlayer.worldRoot == null)
        {
            GameObject obj = new GameObject("root");            
            dbPlayer.worldRoot = obj.transform;
        }

        divideChunkSize = 1.0f / GM.mng.chunkSize;
        // CheckChunk().Forget();
    }

    async UniTask CheckChunk()
    {
        while (true)
        {
            var (x, y) = fg.GetChunk2(transform.position, divideChunkSize);

            if (IsChangedChunk(x, y))
            {
                dbPlayer.chunkX = x;
                dbPlayer.chunkY = y;
                await GM.MsgAsync("LoadWorldByChunk", x, y);
            }

            await UniTask.Yield();
        }
    }

    bool IsChangedChunk(int x, int y)
    {
        return !(dbPlayer.chunkX == x && dbPlayer.chunkY == y);
    }
}
