using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DC;
using MemoryPack;
using UnityEngine;

/// <summary>
/// 全Objectの座標管理
/// </summary>
public class RTCObjectSyncManager : MonoBehaviour
{
    [SerializeField] GameObject emptyPrefab;
    [SerializeField] GameObject playerPrefab;
    private Dictionary<string, List<string>> syncObjectsByID;
    private Dictionary<string, RTCObjectSync> syncObjects;
    HashSet<string> requestObjWaitingList = new();

    void Start()
    {
        syncObjectsByID = GM.db.rtc.syncObjectsByID;
        syncObjects = GM.db.rtc.syncObjects;

        // GM.Add<string, string>("RequestObjectData", RequestObjectData);
        GM.Add<RTCMessage, string, string>($"RECV_{nameof(MessageType.Location)}", ReceiveLocation);
        GM.Add<RTCMessage, string, string>($"RECV_{nameof(MessageType.ObjectChange)}", ObjectChange);
        GM.Add<RTCMessage, string, string>($"RECV_{nameof(MessageType.Animation)}", Animation);
        GM.Add<RTCMessage, string, string>($"RECV_{nameof(MessageType.ObjectInfoRequest)}", ReceiveObjectInfoRequest);
        GM.Add<RTCMessage, string, string>($"RECV_{nameof(MessageType.ObjectInfoResponse)}", ReceiveObjectInfoResponse);
        // GM.Add<byte[], string>("RPC_requestObj", RPC_RTCObjectInfoRequest);
        // GM.Add<RTCMessage, string, string>("RECV_instantiate", RPC_ObjectInstantiate);
        // GM.Add<RTCMessage, string, string>("RECV_ChangeNametag", RPC_ChangeNametag);

        GM.Add<RTCObjectSync>("AddSyncObject", (obj) =>
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

        GM.Add<string>("UserObjectInfoRequest", (connectedId) =>
        {
            // UserObjectの情報を取得する
            var objInfoRequest = new P_ObjectInfoRequest()
            {
                objId = "", // 何も書かない場合はUserObject
            };

            var bytes = MemoryPackSerializer.Serialize(objInfoRequest);
            GM.Msg("RTCSendDirect", MessageType.ObjectInfoRequest, connectedId, bytes);
        });
    }

    /// <summary>
    /// ObjectInfoを返す   
    /// </summary>
    /// <param name="message"></param>
    /// <param name="sourceId"></param>
    /// <param name="_"></param>
    private void ReceiveObjectInfoRequest(RTCMessage message, string sourceId, string _)
    {
        var requestData = MemoryPackSerializer.Deserialize<P_ObjectInfoRequest>(message.data);
        var objId = requestData.objId;

        if (string.IsNullOrEmpty(objId))
        {
            // UserObjectのRequestの場合
            objId = GM.db.rtc.selfObject.objId;
        }

        if (!syncObjects.TryGetValue(objId, out RTCObjectSync obj))
        {
            // Objectが存在しない
            var error = new P_Error
            {
                message = $"Not found objId: {objId}",
            };
            var errorBytes = MemoryPackSerializer.Serialize(error);

            GM.Msg("RTCSendDirect", MessageType.Error, sourceId, errorBytes);
            return;
        }

        var bytes = GetObjectData(obj.gameObject, objId, obj.cid);

        GM.Msg("RTCSendDirect", MessageType.ObjectInfoResponse, sourceId, bytes);
    }

    /// <summary>
    /// Responseを受け取って、Objectを生成する
    /// </summary>
    /// <param name="message"></param>
    /// <param name="sourceId"></param>
    /// <param name="_"></param>
    private void ReceiveObjectInfoResponse(RTCMessage message, string sourceId, string _)
    {
        var responseData = MemoryPackSerializer.Deserialize<P_ObjectInfoResponse>(message.data);

        if (syncObjects.ContainsKey(responseData.objId))
        {
            // 既に存在するObject
            Debug.LogWarning("既に存在するObject");
            return;
        }

        InstantiateObject(message.data, sourceId, true);

        requestObjWaitingList.Remove(responseData.objId);
        // Avatar設定
        // GM.db.user.AddUser(sourceId, objId); // TODO: 実行順番に注意
        // await SetAvatar(objId, cid);
    }

    /// <summary>
    /// 再帰的にObjectの情報を取得する
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="objId"></param>
    /// <param name="cid"></param>
    /// <returns></returns>
    byte[] GetObjectData(GameObject obj, string objId, string cid)
    {
        // 子Objectの情報を取得
        List<byte[]> childBytes = new();
        foreach (Transform child in obj.transform)
        {
            var childByte = GetObjectData(child.gameObject, objId, cid);
            childBytes.Add(childByte);
        }

        // GameObjectからすべてのコンポーネントを取得
        Component[] components = obj.GetComponents<Component>();
        var componentNames = new List<string>();
        foreach (Component component in components)
        {
            string componentName = component.GetType().Name;
            componentNames.Add(componentName);
        }

        var objectInfo = new P_ObjectInfoResponse
        {
            objId = objId,
            position = obj.transform.position,
            rotation = obj.transform.rotation.eulerAngles,
            cid = cid,
            objName = obj.name,
            childObjId = childBytes,
            components = componentNames,
        };

        return MemoryPackSerializer.Serialize(objectInfo);
    }

    Transform InstantiateObject(byte[] bytes, string sourceId, bool rootObject = false)
    {
        var responseData = MemoryPackSerializer.Deserialize<P_ObjectInfoResponse>(bytes);
        var objId = responseData.objId;

        // Object生成
        // TODO: typeに応じてobjectの種類を変えるべき
        var obj = new GameObject(responseData.objName);

        // Object設定
        if (rootObject)
        {
            var objectData = obj.AddComponent<RTCObjectSync>();
            objectData.SetData(sourceId, responseData);

            AddSyncObject(sourceId, objId, obj, objectData);
        }

        // Component設定
        var components = responseData.components;
        foreach (var component in components)
        {
            var componentType = Type.GetType(component);
            obj.AddComponent(componentType);
        }

        // Child Objects設定
        var childObjs = responseData.childObjId;
        foreach (var childData in childObjs)
        {
            var childTransform = InstantiateObject(childData, sourceId);
            childTransform.SetParent(obj.transform);
        }

        return obj.transform;
    }

    private void AddSyncObject(string sourceId, string objId, GameObject obj, RTCObjectSync objectData)
    {
        syncObjects.TryAdd(objId, objectData);
        syncObjectsByID.TryAdd(sourceId, new List<string>());
        if (!syncObjectsByID[sourceId].Contains(objId))
        {
            syncObjectsByID[sourceId].Add(objId);
        }

        GM.db.rtc.activeObjects.TryAdd(objId, obj);
    }

    private void Animation(RTCMessage data, string sourceId, string _)
    {
        var animationData = MemoryPackSerializer.Deserialize<P_Animation>(data.data);
        if (!IsExistObject(sourceId, animationData.objId)) return;

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
    }

    /// <summary>
    /// 名前の変更
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sourceId"></param>
    [Obsolete]
    private void RPC_ChangeNametag(RTCMessage data, string sourceId, string _)
    {
        var changeData = MemoryPackSerializer.Deserialize<P_ChangeNametag>(data.data);
        var objId = changeData.objId;
        if (!syncObjects.TryGetValue(objId, out RTCObjectSync obj)) return;
        obj.nametag = changeData.nametag;
    }

    /// <summary>
    /// 座標情報の取得
    /// </summary>
    /// <param name="data"></param>
    void ReceiveLocation(RTCMessage data, string targetId, string _)
    {
        var locationData = MemoryPackSerializer.Deserialize<P_Location>(data.data);
        var objId = locationData.objId;
        if (!IsExistObject(targetId, objId)) return;

        syncObjects[objId].ReceiveLocation(locationData);
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
    //async void RPC_ObjectInstantiate(RTCMessage data, string sourceId, string _)
    //{
    //    var objectInstantiateData = MemoryPackSerializer.Deserialize<P_ObjectInstantiate>(data.data);
    //    // 情報受け取り
    //    var objId = objectInstantiateData.objId;
    //    if (syncObjects.ContainsKey(objId))
    //    {
    //        // 既に存在するObject
    //        Debug.LogWarning("既に存在するObject");
    //        return;
    //    }

    //    // requestObjWaitingList.Remove(objId);

    //    var position = objectInstantiateData.position;
    //    var rotation = objectInstantiateData.rotation;
    //    var cid = objectInstantiateData.cid;
    //    var objType = objectInstantiateData.objType;
    //    var nameTag = objectInstantiateData.nameTag;

    //    // Object生成
    //    // TODO: typeに応じてobjectの種類を変えるべき
    //    GameObject obj = null;
    //    if (objType == "human")
    //    {
    //        obj = Instantiate(playerPrefab);
    //    }
    //    else
    //    {
    //        obj = Instantiate(emptyPrefab);
    //    }

    //    // Object設定
    //    var objectData = obj.AddComponent<RTCObjectSync>();
    //    objectData.SetData(sourceId, objId, cid, objType, position, rotation);
    //    Debug.Log(nameTag);
    //    objectData.nametag = nameTag;

    //    syncObjects.TryAdd(objId, objectData);
    //    syncObjectsByID.TryAdd(sourceId, new List<string>());
    //    if (!syncObjectsByID[sourceId].Contains(objId))
    //    {
    //        syncObjectsByID[sourceId].Add(objId);
    //    }

    //    GM.db.rtc.activeObjects.Add(objId, obj);

    //    // Avatar設定
    //    GM.db.user.AddUser(sourceId, objId); // TODO: 実行順番に注意
    //    await SetAvatar(objId, cid);
    //}

    /// <summary>
    /// Objectの切り替え
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sourceId"></param>
    void ObjectChange(RTCMessage data, string sourceId, string _)
    {
        var objectChangeData = MemoryPackSerializer.Deserialize<P_ObjectChange>(data.data);
        var objId = objectChangeData.objId;
        if (!IsExistObject(sourceId, objId)) return;
        var cid = objectChangeData.cid;

        // await SetAvatar(objId, cid);
    }

    [Obsolete]
    async UniTask SetAvatar(string objId, string cid)
    {
        if (string.IsNullOrEmpty(cid)) return;

        // Load Avatar
        var obj = syncObjects[objId];
        var avatar = await GM.Msg<UniTask<GameObject>>("DownloadAvatar", cid);
        obj.SetContent(avatar.transform);
    }

    bool IsExistObject(string sourceId, string objId)
    {
        if (objId == null)
        {
            Debug.LogWarning("objId is null");
            return false;
        }

        if (syncObjects.ContainsKey(objId)) return true;

        if (requestObjWaitingList.Contains(objId)) return false;

        requestObjWaitingList.Add(objId);

        var objInfoRequest = new P_ObjectInfoRequest()
        {
            objId = objId,
        };

        var bytes = MemoryPackSerializer.Serialize(objInfoRequest);
        GM.Msg("RTCSendDirect", MessageType.ObjectInfoRequest, sourceId, bytes);

        return false;
    }

    /// <summary>
    /// 新規Object
    /// </summary>
    /// <param name="objId"></param>
    [Obsolete]
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

        GM.Msg("RPCSendDirect", targetId, sendData);
    }
}
