using Cysharp.Threading.Tasks;
using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        GM.Add<string, (int, int, int)>("AddChunk", AddChunk);
        GM.Add<Dictionary<string, object>>("SetSelfLocationData", (data) => selfLocationData = data);
        GM.Add<Dictionary<string, object>>("SetSelfAnimationData", (data) => selfAnimationData = data);

        // 決められた時間ごとに送信
        foreach (var distance in GM.db.rtc.classifiedDistances)
        {
            SendLocation(distance).Forget();
            SendAnimation(distance).Forget();
        }
    }

    private void AddChunk(string id, (int, int, int) chunk)
    {
        GM.db.chunk.chunkTable.ForceAdd(id, chunk);

        // 有効Chunkないかどうかチェック
        if (!GM.db.chunk.sendTargetChunk.Contains(chunk)) return;

        GM.db.rtc.classifiedNodes[GM.db.rtc.classifiedDistances[0]].Add(id);
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
