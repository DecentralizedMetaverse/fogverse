using Cysharp.Threading.Tasks;
using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 距離に応じて時間ごとに位置情報を送信する
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
        // 決められた時間ごとに送信
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

        // どの範囲に属するか調べる
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

            // 範囲内に入った場合
            var syncDistance = GM.db.rtc.classifiedDistances[i];

            // 前回の範囲と違う場合
            Debug.Log($"[{i}] {distance} {compareDistance} {syncDistance} {obj.syncDistance}");
            if (syncDistance != obj.syncDistance)
            {

                if (obj.syncDistance != 0)
                {
                    GM.db.rtc.classifiedNodes[obj.syncDistance].Remove(id);
                }

                // 更新
                GM.db.rtc.classifiedNodes[syncDistance].Add(id);
                obj.syncDistance = syncDistance;

                Debug.Log($"距離: {distance}");
            }
            break;
        }
    }

    /// <summary>
    /// Chunkを更新する
    /// </summary>
    /// <param name="id"></param>
    /// <param name="chunk"></param>
    private void UpdateChunk(string id, (int, int, int) chunk)
    {
        // Chunkを更新
        GM.db.chunk.chunkTable.ForceAdd(id, chunk);

        // 有効Chunk内かどうかチェック
        if (!GM.db.chunk.sendTargetChunk.Contains(chunk))
        {
            // TODO: 有効範囲外の場合は,削除するか検討           
            return;
        }

        var obj = GM.db.rtc.GetSyncObjectById(id);
        if (obj == null || obj.syncDistance == 0)
        {
            // まだ、同期対象になっていない場合
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
