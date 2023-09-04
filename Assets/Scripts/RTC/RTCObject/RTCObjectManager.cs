using Cysharp.Threading.Tasks;
using DC;
using MemoryPack;
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
    // HashSet<string> requestObjWaitingList = new();

    void Start()
    {
        syncObjectsByID = GM.db.rtc.syncObjectsByID;
        syncObjects = GM.db.rtc.syncObjects;

        GM.Add<string, string>("RequestObjectData", RequestObjectData);
        GM.Add<RTCMessage, string>("RECV_Location", RPC_ReceiveLocation);
        // GM.Add<byte[], string>("RPC_requestObj", RPC_RTCObjectInfoRequest);
        GM.Add<RTCMessage, string>("RECV_instantiate", RPC_ObjectInstantiate);
        GM.Add<RTCMessage, string>("RECV_change", RPC_ObjectChange);
        GM.Add<RTCMessage, string>("RECV_ChangeNametag", RPC_ChangeNametag);
        GM.Add<RTCMessage, string>("RECV_Animation", (data, sourceId) =>
        {
            var animationData = MemoryPackSerializer.Deserialize<P_Animation>(data.data);
            if (!syncObjects.ContainsKey(animationData.objId))
            {
                Debug.LogWarning("存在しないObjectへのアクセス");
                return;
            }

            if (syncObjects[animationData.objId].rtcAniamtor == null)
            {
                Debug.LogWarning("Not found RTC Animator");
                return;
            }

            syncObjects[animationData.objId].rtcAniamtor.ReceiveAnim(animationData);
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
    private void RPC_ChangeNametag(RTCMessage data, string sourceId)
    {
        var changeData = MemoryPackSerializer.Deserialize<P_ChangeNametag>(data.data);
        var objId = changeData.objId;
        if (!syncObjects.TryGetValue(objId, out RTCObject obj)) return;
        obj.nametag = changeData.nametag;
    }

    /// <summary>
    /// 座標情報の取得
    /// </summary>
    /// <param name="data"></param>
    void RPC_ReceiveLocation(RTCMessage data, string targetId)
    {
        var locationData = MemoryPackSerializer.Deserialize<P_Location>(data.data);
        var objId = locationData.objId;
        if (!syncObjects.ContainsKey(objId))
        {
            // 新規Object
            RequestObjectData(targetId, objId);
            return;
        }

        syncObjects[objId].ReceiveLocation(locationData);
    }

    /// <summary>
    /// 新規Object
    /// </summary>
    /// <param name="objId"></param>
    void RequestObjectData(string targetId, string objId)
    {
        //if (requestObjWaitingList.Contains(objId)) return;
        //requestObjWaitingList.Add(objId);

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
    //void RPC_RTCObjectInfoRequest(byte[] data, string targetId)
    //{
    //    //var objId = data["objId"].ToString();
    //    if (string.IsNullOrEmpty(objId))
    //    {
    //        // 自分のObjectを取得
    //        var self = GM.db.rtc.selfObject;
    //        if(self != null)
    //        {
    //            objId = self.objId;
    //        }
    //        // TODO: 動作チェック
    //        //if (syncObjectsByID.TryGetValue(GM.db.rtc.id, out List<string> objIds))
    //        //{
    //        //    objId = objIds[0];
    //        //}
    //    }

    //    if (!syncObjects.TryGetValue(objId, out RTCObject obj))
    //    {
    //        // Error
    //        GM.db.rtc.errorData.ForceAdd("reason", "存在しないObjectへのAccess");
    //        GM.Msg("RTCSendDirect", targetId, GM.db.rtc.errorData);
    //        Debug.LogWarning("存在しないObjectへのアクセス");
    //        return;
    //    }

    //    Dictionary<string, object> sendData = new()
    //    {
    //        { "objId",  objId },
    //        { "type", "instantiate" },
    //        { "objType",  obj.objType },
    //        { "cid", obj.cid },
    //        { "nametag", obj.nametag },
    //        { "position", obj.transform.position.ToSplitString() },
    //        { "rotation", obj.transform.rotation.eulerAngles.ToSplitString() },
    //    };

    //    GM.Msg("RTCSendDirect", targetId, sendData);
    //}

    /// <summary>
    /// Object生成
    /// </summary>
    /// <param name="data"></param>
    async void RPC_ObjectInstantiate(RTCMessage data, string sourceId)
    {
        var objectInstantiateData = MemoryPackSerializer.Deserialize<P_ObjectInstantiate>(data.data);    
        // 情報受け取り
        var objId = objectInstantiateData.objId;
        if(syncObjects.ContainsKey(objId))
        {
            // 既に存在するObject
            Debug.LogWarning("既に存在するObject");
            return;
        }

        // requestObjWaitingList.Remove(objId);

        var position = objectInstantiateData.position;
        var rotation = objectInstantiateData.rotation;
        var cid = objectInstantiateData.cid;
        var objType = objectInstantiateData.objType;
        var nameTag = objectInstantiateData.nameTag;

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
        objectData.SetData(sourceId, objId, cid, objType, position, rotation);
        Debug.Log(nameTag);
        objectData.nametag = nameTag;

        syncObjects.TryAdd(objId, objectData);
        syncObjectsByID.TryAdd(sourceId, new List<string>());
        if (!syncObjectsByID[sourceId].Contains(objId))
        {
            syncObjectsByID[sourceId].Add(objId);
        }

        GM.db.rtc.activeObjects.Add(objId, obj);

        // Avatar設定
        GM.db.user.AddUser(sourceId, objId); // TODO: 実行順番に注意
        await SetAvatar(objId, cid);
    }

    /// <summary>
    /// Objectの切り替え
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sourceId"></param>
    async void RPC_ObjectChange(RTCMessage data, string sourceId)
    {
        var objectChangeData = MemoryPackSerializer.Deserialize<P_ObjectChange>(data.data);
        var objId = objectChangeData.objId;
        var cid = objectChangeData.cid;;

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
