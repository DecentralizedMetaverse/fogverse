using Cysharp.Threading.Tasks;
using DC;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全Objectの座標管理
/// TODO: 初期表示されない
/// </summary>
public class RTCObjectManager : MonoBehaviour
{
    [SerializeField] GameObject emptyPrefab;
    [SerializeField] GameObject playerPrefab;
    private Dictionary<string, List<string>> syncObjectsByID;
    private Dictionary<string, RTCObject> syncObjects;

    void Start()
    {
        syncObjectsByID = GM.db.rtc.syncObjectsByID;
        syncObjects = GM.db.rtc.syncObjects;

        GM.Add<string, string>("RequestObjectData", RequestObjectData);
        GM.Add<Dictionary<string, object>, string>("RPC_location", RPC_ReceiveLocation);
        GM.Add<Dictionary<string, object>, string>("RPC_requestObj", RPC_RTCRequstObjectInfo);
        GM.Add<Dictionary<string, object>, string>("RPC_instantiate", RPC_ObjectInstantiate);
        GM.Add<Dictionary<string, object>, string>("RPC_change", RPC_ObjectChange);
        GM.Add<Dictionary<string, object>, string>("RPC_changeNametag", RPC_ChangeNametag);
        GM.Add<Dictionary<string, object>, string>("RPC_anim", (data, sourceId) =>
        {
            if (!syncObjects.ContainsKey(data["objId"].ToString()))
            {
                Debug.LogWarning("存在しないObjectからのアクセス");
                return;
            }


            if (syncObjects[data["objId"].ToString()].rtcAniamtor == null)
            {
                Debug.LogWarning("Not found rtc Animator");
                return;
            }

            syncObjects[data["objId"].ToString()].rtcAniamtor.ReceiveAnim(data);
        });

        GM.Add<RTCObject>("AddSyncObject", (obj) =>
        {
            // LocalAvatarを追加する
            if (syncObjects.ContainsKey(obj.objId)) return;
            syncObjects.Add(obj.objId, obj);
            syncObjectsByID.TryAdd(GM.db.rtc.id, new List<string>());
            syncObjectsByID[GM.db.rtc.id].Add(obj.objId);
            GM.db.user.AddUser(GM.db.rtc.id, obj.objId);
        });
        GM.Add<string>("DestroySyncObject", (id) =>
        {
            if (!syncObjectsByID.ContainsKey(id)) return;

            foreach (var objId in syncObjectsByID[id])
            {
                Destroy(syncObjects[objId].gameObject);
                syncObjects.Remove(objId);
            }
            syncObjectsByID.Remove(id);
        });
    }

    /// <summary>
    /// 名前の変更
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sourceId"></param>
    private void RPC_ChangeNametag(Dictionary<string, object> data, string sourceId)
    {
        var objId = data["objId"].ToString();
        if (!syncObjects.TryGetValue(objId, out RTCObject obj)) return;
        obj.nametag = data["nametag"].ToString();
    }

    /// <summary>
    /// 座標情報の取得
    /// </summary>
    /// <param name="data"></param>
    void RPC_ReceiveLocation(Dictionary<string, object> data, string targetId)
    {
        var objId = data["objId"].ToString();
        if (!syncObjects.ContainsKey(objId))
        {
            // 新規Object
            RequestObjectData(targetId, objId);
            return;
        }

        syncObjects[objId].ReceiveLocation(data);
    }

    /// <summary>
    /// 新規Object
    /// </summary>
    /// <param name="objId"></param>
    void RequestObjectData(string targetId, string objId)
    {
        Dictionary<string, object> sendData = new()
        {
            { "objId", objId },
            { "type", "requestObj" },
            //{ "avatarId", "..." },
        };

        GM.Msg("RTCSendDirect", targetId, sendData);
    }

    /// <summary>
    /// Objectに関する情報の取得
    /// </summary>
    /// <param name="data"></param>
    /// <param name="targetId"></param>
    void RPC_RTCRequstObjectInfo(Dictionary<string, object> data, string targetId)
    {
        var objId = data["objId"].ToString();
        if (string.IsNullOrEmpty(objId))
        {
            if (syncObjectsByID.TryGetValue(GM.db.rtc.id, out List<string> objIds))
            {
                objId = objIds[0];
            }
        }

        if (!syncObjects.TryGetValue(objId, out RTCObject obj))
        {
            // Error
            GM.Msg("RTCSendDirect", targetId, GM.db.rtc.errorData);
            return;
        }

        Dictionary<string, object> sendData = new()
        {
            { "objId",  objId },
            { "type", "instantiate" },
            { "objType",  obj.objType },
            { "cid", obj.cid },
            { "nametag", obj.nametag },
            { "position", obj.transform.position.ToSplitString() },
            { "rotation", obj.transform.rotation.eulerAngles.ToSplitString() },
        };

        GM.Msg("RTCSendDirect", targetId, sendData);
    }

    /// <summary>
    /// Object生成
    /// </summary>
    /// <param name="data"></param>
    async void RPC_ObjectInstantiate(Dictionary<string, object> data, string sourceId)
    {
        // 情報受け取り
        var objId = data["objId"].ToString();
        var position = data["position"].ToString().ToVector3();
        var rotation = data["rotation"].ToString().ToVector3();
        var cid = data["cid"].ToString();
        var objType = data["objType"].ToString();
        var nameTag = data["nametag"].ToString();

        // Object生成
        // TODO: typeに応じてobjectの種類を変えるべき
        GameObject obj = null;
        if (objType == "human")
        {
            obj = Instantiate(playerPrefab);
        }
        else
        {
            obj = Instantiate(emptyPrefab);
        }

        // Object設定
        var objectData = obj.AddComponent<RTCObject>();
        objectData.SetData(objId, cid, objType, position, rotation);
        Debug.Log(nameTag);
        objectData.nametag = nameTag;

        syncObjects.TryAdd(objId, objectData);
        syncObjectsByID.TryAdd(sourceId, new List<string>());
        if (!syncObjectsByID[sourceId].Contains(objId))
        {
            syncObjectsByID[sourceId].Add(objId);
        }

        // Avatar設定
        GM.db.user.AddUser(sourceId, objId); // TODO: 実行順番に注意
        await SetAvatar(objId, cid);
    }

    /// <summary>
    /// Objectの切り替え
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sourceId"></param>
    async void RPC_ObjectChange(Dictionary<string, object> data, string sourceId)
    {
        var objId = data["objId"].ToString();
        var cid = data["cid"].ToString();

        if (!syncObjects.ContainsKey(objId))
        {
            // 新規Object
            RequestObjectData(sourceId, objId);
            return;
        }

        await SetAvatar(objId, cid);
    }

    async UniTask SetAvatar(string objId, string cid)
    {
        if (string.IsNullOrEmpty(cid)) return;

        // Load Avatar
        var obj = syncObjects[objId];
        var avatar = await GM.Msg<UniTask<GameObject>>("DownloadAvatar", cid);
        obj.SetContent(avatar.transform);
    }
}
