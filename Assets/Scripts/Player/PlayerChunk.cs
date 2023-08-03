using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using DC;
using UnityEngine;
using System.Threading;

/// <summary>
/// プレイヤーのチャンクを常時調べる
/// 変化時はWorldの読み込みを行う
/// TODO: UniTaskのCancel処理を行う
/// </summary>
[RequireComponent(typeof(RTCObject))]
public class PlayerChunk : MonoBehaviour
{
    [SerializeField] DB_Player dbPlayer;
    float divideChunkSize;
    private CancellationTokenSource cts;
    RTCObject rtc;

    void Start()
    {
        rtc = GetComponent<RTCObject>();
        if (!rtc.isLocal) return;

        dbPlayer.user = transform;
        if(dbPlayer.worldRoot == null)
        {
            GameObject obj = new GameObject("root");            
            dbPlayer.worldRoot = obj.transform;
        }

        divideChunkSize = 1.0f / GM.db.chunk.chunkSize;
        
        cts = new CancellationTokenSource();
        UpdateCheckChunk(cts.Token).Forget();
    }

    void OnDestroy()
    {
        if (cts != null) cts.Cancel();
    }

    async UniTask UpdateCheckChunk(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            if (cancellationToken.IsCancellationRequested) return;

            var chunk = fg.GetChunk(transform.position, divideChunkSize);

            if (IsChangedChunk(chunk))
            {
                dbPlayer.chunk = chunk;
                // Worldを読み込む
                // await GM.MsgAsync("LoadWorldByChunk", x, y);

                // Chunkの変更を送信
                GM.Msg("ChangeChunk", chunk);
            }

            await UniTask.Yield();
        }
    }

    bool IsChangedChunk((int, int, int) chunk)
    {
        return dbPlayer.chunk != chunk;
    }
}
