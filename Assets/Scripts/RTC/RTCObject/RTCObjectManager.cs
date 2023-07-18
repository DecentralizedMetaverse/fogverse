using Cysharp.Threading.Tasks;
using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// �SObject�̍��W�Ǘ�
/// TODO: �����\������Ȃ�
/// </summary>
public class RTCObjectManager : MonoBehaviour
{
    [SerializeField] GameObject prefab;
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
        GM.Add<RTCObject>("AddSyncObject", (obj) =>
        {
            // LocalAvatar��ǉ�����
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
    /// ���W���̎擾
    /// </summary>
    /// <param name="data"></param>
    void RPC_ReceiveLocation(Dictionary<string, object> data, string targetId)
    {
        var objId = data["objId"].ToString();
        if (!syncObjects.ContainsKey(objId))
        {
            // �V�KObject
            RequestObjectData(targetId, objId);
            return;
        }

        syncObjects[objId].ReceiveLocation(data);
    }

    /// <summary>
    /// �V�KObject
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

    void RPC_RTCRequstObjectInfo(Dictionary<string, object> data, string targetId)
    {
        var objId = data["objId"].ToString();
        if(string.IsNullOrEmpty(objId))
        {
            if(syncObjectsByID.TryGetValue(GM.db.rtc.id, out List<string> objIds))
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
            { "cid", obj.cid },
            { "position", obj.transform.position.ToSplitString() },
            { "rotation", obj.transform.rotation.eulerAngles.ToSplitString() },
        };

        GM.Msg("RTCSendDirect", targetId, sendData);
    }

    /// <summary>
    /// Object����
    /// </summary>
    /// <param name="data"></param>
    async void RPC_ObjectInstantiate(Dictionary<string, object> data, string sourceId)
    {
        // ���󂯎��
        var objId = data["objId"].ToString();
        var position = data["position"].ToString().ToVector3();
        var rotation = data["rotation"].ToString().ToVector3();
        var cid = data["cid"].ToString();

        // Object����
        var obj = Instantiate(prefab);

        // Object�ݒ�
        var objectData = obj.AddComponent<RTCObject>();
        objectData.SetData(objId, cid, position, rotation);

        syncObjects.TryAdd(objId, objectData);
        syncObjectsByID.TryAdd(sourceId, new List<string>());
        if (!syncObjectsByID[sourceId].Contains(objId))
        {
            syncObjectsByID[sourceId].Add(objId);
        }

        // Avatar�ݒ�
        GM.db.user.AddUser(sourceId, objId); // TODO: ���s���Ԃɒ���
        await SetAvatar(objId, cid);
    }

    /// <summary>
    /// Object�̐؂�ւ�
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sourceId"></param>
    async void RPC_ObjectChange(Dictionary<string, object> data, string sourceId)
    {
        var objId = data["objId"].ToString();
        var cid = data["cid"].ToString();

        if (!syncObjects.ContainsKey(objId))
        {
            // �V�KObject
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
