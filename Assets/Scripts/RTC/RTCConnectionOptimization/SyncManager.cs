using Cysharp.Threading.Tasks;
using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// �����ɉ����Ď��Ԃ��ƂɈʒu���𑗐M����
/// </summary>
public class SyncManager : MonoBehaviour
{
    Dictionary<string, GameObject> activeObjects;
    Dictionary<string, object> selfLocationData = new();
    Dictionary<string, object> selfAnimationData = new();

    private void Start()
    {
        GM.Add<string, (int, int, int)>("UpdateChunk", UpdateChunk);
        GM.Add<Dictionary<string, object>>("SetSelfLocationData", (data) => selfLocationData = data);
        GM.Add<Dictionary<string, object>>("SetSelfAnimationData", (data) => selfAnimationData = data);
        GM.Add<string, Vector3>("UpdateDistance", UpdateDistance);
        // ���߂�ꂽ���Ԃ��Ƃɑ��M
        foreach (var distance in GM.db.rtc.classifiedDistances)
        {
            SendLocation(distance).Forget();
            SendAnimation(distance).Forget();
        }
    }

    private void UpdateDistance(string id, Vector3 position)
    {
        var self = GM.db.rtc.selfObject;
        var distance = Vector3.Distance(self.transform.position, position);

        // �ǂ͈̔͂ɑ����邩���ׂ�
        for (var i = 0; i < GM.db.rtc.classifiedDistances.Count; i++)
        {
            var compareDistance = GM.db.rtc.classifiedDistances[i];
            if (distance > compareDistance) continue;

            var obj = GM.db.rtc.GetSyncObjectById(id);
            if (obj == null)
            {
                Debug.LogWarning("Not found object");
                return;
            }

            // �͈͓��ɓ������ꍇ
            var syncDistance = GM.db.rtc.classifiedDistances[i];

            // �O��͈̔͂ƈႤ�ꍇ
            Debug.Log($"[{i}] {distance} {compareDistance} {syncDistance} {obj.syncDistance}");
            if (syncDistance != obj.syncDistance)
            {

                if (obj.syncDistance != 0)
                {
                    GM.db.rtc.classifiedNodes[obj.syncDistance].Remove(id);
                }

                // �X�V
                GM.db.rtc.classifiedNodes[syncDistance].Add(id);
                obj.syncDistance = syncDistance;

                Debug.Log($"����: {distance}");
            }
            break;
        }
    }

    /// <summary>
    /// Chunk���X�V����
    /// </summary>
    /// <param name="id"></param>
    /// <param name="chunk"></param>
    private void UpdateChunk(string id, (int, int, int) chunk)
    {
        // Chunk���X�V
        GM.db.chunk.chunkTable.ForceAdd(id, chunk);

        // �L��Chunk�����ǂ����`�F�b�N
        if (!GM.db.chunk.sendTargetChunk.Contains(chunk))
        {
            // TODO: �L���͈͊O�̏ꍇ��,�폜���邩����           
            return;
        }

        var obj = GM.db.rtc.GetSyncObjectById(id);
        if (obj == null || obj.syncDistance == 0)
        {
            // �܂��A�����ΏۂɂȂ��Ă��Ȃ��ꍇ
            GM.db.rtc.classifiedNodes[GM.db.rtc.classifiedDistances[0]].Add(id);
        }
    }

    /// <summary>
    /// Send location data to all peers
    /// </summary>
    /// <param name="intervalDistance"></param>
    /// <returns></returns>
    async UniTask SendLocation(int intervalDistance)
    {
        var intervalTimeSec = GM.db.rtc.classifiedTimes[intervalDistance];
        while (true)
        {
            var list = GM.db.rtc.classifiedNodes[intervalDistance];
            foreach (var id in list)
            {
                selfLocationData.ForceAdd("time", intervalTimeSec);
                GM.Msg("RTCSendDirect", id, selfLocationData);
            }
            await UniTask.Delay(System.TimeSpan.FromSeconds(intervalTimeSec));
        }
    }

    /// <summary>
    /// Send location data to all peers
    /// </summary>
    /// <param name="intervalDistance"></param>
    /// <returns></returns>
    async UniTask SendAnimation(int intervalDistance)
    {
        var intervalTimeSec = GM.db.rtc.classifiedTimes[intervalDistance];
        while (true)
        {
            var list = GM.db.rtc.classifiedNodes[intervalDistance];
            foreach (var id in list)
            {
                GM.Msg("RTCSendDirect", id, selfAnimationData);
            }
            await UniTask.Delay(System.TimeSpan.FromSeconds(intervalTimeSec));
        }
    }
}
