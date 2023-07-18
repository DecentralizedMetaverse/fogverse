using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using DC;
using UnityEngine;

/// <summary>
/// �v���C���[�̃`�����N���펞���ׂ�
/// �ω�����World�̓ǂݍ��݂��s��
/// TODO: UniTask��Cancel�������s��
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
