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
    Dictionary<string, GameObject> excludedObjects = new();  // 表示制限により、座標同期を停止するObujects
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
            // Debug.Log($"[{i}] {distance} {compareDistance} {syncDistance} {obj.syncDistance}");
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

        if (IsMaxReachedNumShowClient())
        {
            OptimizedRemoveShowClient();
        }
        else if(IsFullNumOfShowClient())
        {            
        }
        else if (HasExcludeObject())
        {
            foreach(var (objId, obj) in excludedObjects)
            {
                obj.SetActive(true);
                excludedObjects.Remove(objId);
                break;
            }
        }

        if (IsMaxNumReservedClient())
        {
            OptimizedRemoveReservedClient();
        }
    }

    private bool HasExcludeObject()
    {
        if (excludedObjects.Count > 0) return true;
        return false;
    }

    private bool IsMaxReachedNumShowClient()
    {
        var numShowPeers = GM.db.rtc.activeObjects.Count;
        var numMaxPeers = GM.db.rtc.maxShowPeers;

        if (numShowPeers > numMaxPeers)
        {
            return true;
        }
        return false;
    }

    bool IsFullNumOfShowClient()
    {
        var numShowPeers = GM.db.rtc.activeObjects.Count;
        var numMaxPeers = GM.db.rtc.maxShowPeers;

        if (numShowPeers == numMaxPeers) return true;
        return false;
    }

    private bool IsMaxNumReservedClient()
    {
        var numPeers = GM.db.rtc.peers.Count - GM.db.chunk.sendTargetChunk.Count;
        var numMaxPeers = GM.db.rtc.maxReservedPeers;

        if (numPeers > numMaxPeers) return true;
        return false;
    }

    private void OptimizedRemoveShowClient()
    {       
        for (var i = GM.db.rtc.classifiedDistances.Count - 1; i >= 0; i--)
        {
            var distanceKey = GM.db.rtc.classifiedDistances[i];
            var clients = GM.db.rtc.classifiedNodes[distanceKey];
            foreach (var id in clients)
            {                
                // 送信対象から外す
                var obj = GM.db.rtc.activeObjects[id];
                excludedObjects.Add(id, obj);
                obj.SetActive(false);                
                return;
            }
        }
    }

    private static bool HasOutOfTargetChunkNode()
    {
        return GM.db.rtc.activeObjects.Count != GM.db.rtc.peers.Count;
    }

    private void OptimizedRemoveReservedClient()
    {
        foreach (var (id, peer) in GM.db.rtc.peers)
        {
            var obj = GM.db.rtc.GetSyncObjectById(id);
            if (obj == null || obj.syncDistance == 0)
            {
                peer.Close();
                break;
            }
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
                if(excludedObjects.ContainsKey(id)) continue;
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
                if (excludedObjects.ContainsKey(id)) continue;
                GM.Msg("RTCSendDirect", id, selfAnimationData);
            }
            await UniTask.Delay(System.TimeSpan.FromSeconds(intervalTimeSec));
        }
    }
}
