using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using DC;
using UnityEngine;
using System.Threading;

/// <summary>
/// �v���C���[�̃`�����N���펞���ׂ�
/// �ω�����World�̓ǂݍ��݂��s��
/// TODO: UniTask��Cancel�������s��
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
                // World��ǂݍ���
                // await GM.MsgAsync("LoadWorldByChunk", x, y);

                // Chunk�̕ύX�𑗐M
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
