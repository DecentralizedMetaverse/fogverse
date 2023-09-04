using Cysharp.Threading.Tasks;
using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 動的Objectの座標管理
/// </summary>
public class RTCObject : MonoBehaviour
{
    // Object Data
    public bool isLocal = false;
    [System.NonSerialized] public string objId = "";
    public string ownerId = "";
    public string cid = "";
    public string objType = "human";    // TODO: 別の場所で設定すべき
    public float syncIntervalTimeSecond = 0.1f; // 最小値を設定しておく
    public int syncDistance = 0;

    public string nametag
    {
        get => _nametag;
        set
        {
            _nametag = value;
            nameTag?.SetName(value);
            if (!isLocal) return;

            // 自キャラの場合は、全員に送信する
            var sendData = new Dictionary<string, object>()
            {
                { "type","changeNametag" },
                { "objId", objId },
                { "nametag",value },
            };
            GM.Msg("RTCSendAll", sendData);
        }
    }
    [SerializeField] string _nametag = "";
    RTCNameTag nameTag;

    // -----------------------------------
    // Sync Data
    protected P_Location locationData = new();

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

    /// <summary>
    /// 実際の時間？
    /// </summary>
    float elapsedTime = 0;
    private Transform content;

    // -----------------------------------
    // Humanoid
    public Animator animator;
    public RTCAnimator rtcAniamtor;

    // StarterAssets.StarterAssetsInputs starterAssets;

    // -----------------------------------
    private void Awake()
    {
        if (objType == "human")
        {
            // 人型のObjectであれば、Geometryに設定する
            content = transform.Find("Geometry");
            // TryGetComponent(out animator);
            TryGetComponent(out nameTag);
            if(!TryGetComponent(out rtcAniamtor))
            {
                Debug.LogError("RTCAnimatorが存在しない");
            }
        }
        else
        {
            // 子階層にObject設置の場所を作成する
            GameObject content = new GameObject("content");
            content.transform.SetParent(transform);
            this.content = content.transform;
            content.transform.ResetTransform();
        }
    }

    private void Start()
    {
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
    /// Localの場合、Dataを設定する
    /// </summary>
    public void SetData()
    {
        isLocal = true;
        PrepareSendLocationData();
        objId = Guid.NewGuid().ToString("N");
        locationData.objId = objId;
        ownerId = GM.db.rtc.id;
        gameObject.name = $"{name}-{objId}";

        var tag = GM.Msg<object>("GetSaveData", "nametag");
        if (tag != null)
        {
            nametag = tag.ToString();
        }

        GM.Msg("AddSyncObject", this);
    }

    /// <summary>
    /// Remote
    /// </summary>
    /// <param name="objId"></param>
    /// <param name="cid"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    public void SetData(string id, string objId, string cid, string objType, Vector3 position, Vector3 rotation)
    {
        this.ownerId = id;
        this.objId = objId;
        this.cid = cid;
        this.objType = objType;
        isLocal = false;
        transform.position = position;
        transform.rotation = Quaternion.Euler(rotation);
        gameObject.name = $"{name}-{objId}";
    }

    /// <summary>
    /// Dataを送信する準備
    /// </summary>
    void PrepareSendLocationData()
    {
        locationData.objId = "";
        locationData.position = transform.position;
        locationData.rotation = transform.rotation.eulerAngles;        
    }

    /// <summary>
    /// Prepare Send Data
    /// </summary>
    void UpdataLocation()
    {
        // 送信間隔
        time += Time.deltaTime;
        if (time < syncIntervalTimeSecond) return;
        time = 0;

        // 座標が変わっていない場合は、送信しない
        if (previousPosition == transform.position &&
            previousRotation == transform.rotation) return; 

        // 座標が異なる場合、送信する
        previousPosition = transform.position;
        transform.rotation = transform.rotation;

        locationData.position = transform.position;
        locationData.rotation = transform.rotation.eulerAngles;

        // いつでも自分の情報を送れるように準備しておく
        GM.Msg("SetSelfLocationData", locationData);
    }

    /// <summary>
    /// 座標を受信する
    /// </summary>
    /// <param name="data"></param>
    public void ReceiveLocation(P_Location data)
    {
        recievedPosition = data.position;
        recievedRotation = Quaternion.Euler(data.rotation);
        // syncIntervalTimeSecond = data.time;
        currentPosition = transform.position;
        currentRotation = transform.rotation;
        elapsedTime = 0;
    }

    /// <summary>
    /// 座標の補間
    /// </summary>
    void InterpolationLocation()
    {
        var t = elapsedTime / syncIntervalTimeSecond; // TODO: syncIntervalTimeSecondを外部から取得する必要がある
        elapsedTime += Time.deltaTime;
        transform.position = Vector3.LerpUnclamped(currentPosition, recievedPosition, t);
        transform.rotation = Quaternion.LerpUnclamped(currentRotation, recievedRotation, t);
        GM.Msg("UpdateDistance", ownerId, recievedPosition);
        //if (objType != "human") return;
        //var speed = (currentPosition - transform.position).magnitude;
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
            { "objType",  objType },
            { "cid", cid },
        };
        GM.Msg("RTCSendAll", sendData);
    }

    /// <summary>
    /// Avatar等を設定する
    /// </summary>
    /// <param name="obj"></param>
    public async void SetContent(Transform obj)
    {
        content.DestroyChildren();
        obj.SetParent(content);
        obj.ResetTransform();

        await UniTask.Yield();
        animator?.Rebind();
        animator?.Update(0f);
    }
}
