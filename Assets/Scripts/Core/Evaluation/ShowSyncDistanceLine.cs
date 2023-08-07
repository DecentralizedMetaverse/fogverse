using DC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowSyncDistanceLine : MonoBehaviour
{
    [SerializeField] GameObject linePrefab;

    void Start()
    {
        var player = GM.db.player.user;

        foreach(var (key, value) in GM.db.rtc.classifiedTimes)
        {
            var obj = Instantiate(linePrefab, player);
            obj.transform.ResetTransform();
            obj.transform.localScale = new Vector3(key*2, key*2, key*2);
            obj.name = $"SyncDistanceLine_{key}";
        }
    }    
}
