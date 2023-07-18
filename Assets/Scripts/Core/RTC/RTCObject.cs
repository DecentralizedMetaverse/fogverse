using Cysharp.Threading.Tasks;
using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTCObject : MonoBehaviour
{
    // Object Data
    public bool isLocal = false;
    [System.NonSerialized] public string objId = "";
    public string cid = "";

    // -----------------------------------
    // Sync Data
    protected Dictionary<string, object> locationData = new();

    // -----------------------------------
    // Send
    float time = 0;
    Vector3 previousPosition = Vector3.zero;
    Quaternion previousRotation = Quaternion.identity;

    // -----------------------------------
    // Recieve
    [SerializeField] float interpolationSpeed = 2.0f;
    Vector3 recievedPosition = Vector3.zero;
    Quaternion recievedRotation = Quaternion.identity;

    Vector3 currentPosition = Vector3.zero;
    Quaternion currentRotation = Quaternion.identity;

    float elapsedTime = 0;
    private Transform content;

    // -----------------------------------

    private void Start()
    {
        GameObject content = new GameObject("content");
        content.transform.SetParent(transform);
        this.content = content.transform;
        content.transform.ResetTransform();

        if (!isLocal) return;

        SetData();
    }

    private void Update()
    {
        if (isLocal)
        {
            UpdataLocation();
        }
        else
        {
            InterpolationLocation();
        }
    }

    /// <summary>
    /// Local
    /// </summary>
    public void SetData()
    {
        isLocal = true;
        PrepareSendLocationData();
        objId = Guid.NewGuid().ToString("N");
        locationData["objId"] = objId;

        gameObject.name = $"{name}-{objId}";

        GM.Msg("AddSyncObject", this);
    }

    /// <summary>
    /// Remote
    /// </summary>
    /// <param name="objId"></param>
    /// <param name="cid"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    public void SetData(string objId, string cid, Vector3 position, Vector3 rotation)
    {
        this.cid = cid;
        isLocal = false;
        transform.position = position;
        transform.rotation = Quaternion.Euler(rotation);
    }

    void PrepareSendLocationData()
    {
        locationData.Add("objId", "");
        locationData.Add("type", "location");
        locationData.Add("position", transform.position.ToSplitString());
        locationData.Add("rotation", transform.rotation.eulerAngles.ToSplitString());
    }

    void UpdataLocation()
    {
        time += Time.deltaTime;
        if (time < GM.db.rtc.syncIntervalTimeSecond) return;

        time = 0;

        if (previousPosition == transform.position &&
            previousRotation == transform.rotation) return;

        // 座標が異なる場合、送信する
        previousPosition = transform.position;
        transform.rotation = transform.rotation;

        locationData["position"] = transform.position.ToSplitString();
        locationData["rotation"] = transform.rotation.eulerAngles.ToSplitString();

        GM.Msg("RTCSendAll", locationData);
    }

    public void ReceiveLocation(Dictionary<string, object> data)
    {
        recievedPosition = data["position"].ToString().ToVector3();
        recievedRotation = Quaternion.Euler(data["rotation"].ToString().ToVector3());

        currentPosition = transform.position;
        currentRotation = transform.rotation;
        elapsedTime = 0;
    }

    void InterpolationLocation()
    {
        var t = elapsedTime / GM.db.rtc.syncIntervalTimeSecond;
        elapsedTime += Time.deltaTime;
        transform.position = Vector3.LerpUnclamped(currentPosition, recievedPosition, t);
        transform.rotation = Quaternion.LerpUnclamped(currentRotation, recievedRotation, t);
    }

    /// <summary>
    /// TODO: 誰がこれを送信するかの決定が必要
    /// </summary>
    /// <param name="path"></param>
    public async void SetObject(string path)
    {        
        var avatarObj = GM.Msg<GameObject>("LoadAvatar", path);        
        SetContent(avatarObj.transform);

        // IPFSへのUpload
        var cid = await GM.Msg<UniTask<string>>("UploadAvatar", path);
        this.cid = cid;
        GM.db.user.users[GM.db.rtc.id].cid = cid;

        // 全体への通知
        var sendData = new Dictionary<string, object>()
        {
            { "type", "change" },
            { "objId",  objId },
            { "cid", cid },
        };
        GM.Msg("RTCSendAll", sendData);
    }

    /// <summary>
    /// Avatar等を設定する
    /// </summary>
    /// <param name="obj"></param>
    public void SetContent(Transform obj)
    {
        content.DestroyChildren();
        obj.SetParent(content);
        obj.ResetTransform();
    }
}
