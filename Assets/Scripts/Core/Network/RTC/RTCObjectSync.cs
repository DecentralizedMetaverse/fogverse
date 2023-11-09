using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DC;
using UnityEngine;

/// <summary>
/// ���IObject�̍��W�Ǘ�
/// </summary>
public class RTCObjectSync : MonoBehaviour
{
    // Object Data
    public bool isLocal = false;
    public string ownerId = "";
    public string objId = "";
    public string cid = "";
    public string objType = "human";    // TODO: �ʂ̏ꏊ�Őݒ肷�ׂ�
    public float syncIntervalTimeSecond = 0.1f; // �ŏ��l��ݒ肵�Ă���
    public int syncDistance = 0;

    public string nametag
    {
        get => _nametag;
        set
        {
            _nametag = value;
            nameTag?.SetName(value);
            if (!isLocal) return;

            // ���L�����̏ꍇ�́A�S���ɑ��M����
            var sendData = new Dictionary<string, object>()
            {
                { "type","changeNametag" },
                { "objId", objId },
                { "nametag",value },
            };
            GM.Msg("RPCSendAll", sendData);
        }
    }
    [SerializeField] string _nametag = "";
    public RTCNameTag nameTag;

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
    /// ���ۂ̎��ԁH
    /// </summary>
    float elapsedTime = 0;
    private Transform content;

    // -----------------------------------
    // Humanoid
    public Animator animator;
    public RTCAnimator rtcAniamtor;

    // -----------------------------------
    public Action InitMethod;

    // StarterAssets.StarterAssetsInputs starterAssets;

    // -----------------------------------
    private void Awake()
    {
        if (objType == "human")
        {
            // �l�^��Object�ł���΁AGeometry�ɐݒ肷��
            content = transform.Find("Geometry");
            // TryGetComponent(out animator);
            TryGetComponent(out nameTag);
            if(!TryGetComponent(out rtcAniamtor))
            {
                Debug.LogError("RTCAnimator�����݂��Ȃ�");
            }
        }
        else
        {
            // �q�K�w��Object�ݒu�̏ꏊ���쐬����
            GameObject content = new GameObject("content");
            content.transform.SetParent(transform);
            this.content = content.transform;
            content.transform.ResetTransform();
        }

        if (isLocal)
        {
            SetLocalData();
        }
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
    /// Local�̏ꍇ�AData��ݒ肷��
    /// </summary>
    public void SetLocalData()
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
        InitMethod?.Invoke();
    }

    /// <summary>
    /// Remote
    /// </summary>
    /// <param name="objId"></param>
    /// <param name="cid"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    public void SetData(string ownerId, P_ObjectInfoResponse data)
    {
        this.ownerId = ownerId;
        this.objId = data.objId;
        this.cid = data.cid;
        // this.objType = objType;
        isLocal = false;
        transform.position = data.position;
        transform.rotation = Quaternion.Euler(data.rotation);
        gameObject.name = data.objName;
    }

    /// <summary>
    /// Data�𑗐M���鏀��
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
<<<<<<< Updated upstream:Assets/Scripts/Core/Network/RTC/RTCObjectSync.cs
        // 送信間隔
=======
        // ���M�Ԋu
>>>>>>> Stashed changes:Assets/Scripts/Core/RTC/RTCObject.cs
        time += Time.deltaTime;
        if (time < syncIntervalTimeSecond) return;
        time = 0;

<<<<<<< Updated upstream:Assets/Scripts/Core/Network/RTC/RTCObjectSync.cs
        // 座標が変わっていない場合は、送信しない
=======
        // ���W���ς���Ă��Ȃ��ꍇ�́A���M���Ȃ�
>>>>>>> Stashed changes:Assets/Scripts/Core/RTC/RTCObject.cs
        if (previousPosition == transform.position &&
            previousRotation == transform.rotation) return;

<<<<<<< Updated upstream:Assets/Scripts/Core/Network/RTC/RTCObjectSync.cs
        // 座標が異なる場合、送信する
=======
        // ���W���قȂ�ꍇ�A���M����
>>>>>>> Stashed changes:Assets/Scripts/Core/RTC/RTCObject.cs
        previousPosition = transform.position;
        transform.rotation = transform.rotation;

        locationData.position = transform.position;
        locationData.rotation = transform.rotation.eulerAngles;

<<<<<<< Updated upstream:Assets/Scripts/Core/Network/RTC/RTCObjectSync.cs
        // いつでも自分の情報を送れるように準備しておく
=======
        // ���ł������̏��𑗂��悤�ɏ������Ă���
>>>>>>> Stashed changes:Assets/Scripts/Core/RTC/RTCObject.cs
        GM.Msg("SetSelfLocationData", locationData);
    }

    /// <summary>
<<<<<<< Updated upstream:Assets/Scripts/Core/Network/RTC/RTCObjectSync.cs
    /// 座標を受信する
=======
    /// ���W����M����
>>>>>>> Stashed changes:Assets/Scripts/Core/RTC/RTCObject.cs
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
<<<<<<< Updated upstream:Assets/Scripts/Core/Network/RTC/RTCObjectSync.cs
    /// 座標の補間
    /// </summary>
    void InterpolationLocation()
    {
        var t = elapsedTime / syncIntervalTimeSecond; // TODO: syncIntervalTimeSecondを外部から取得する必要がある
=======
    /// ���W�̕��
    /// </summary>
    void InterpolationLocation()
    {
        var t = elapsedTime / syncIntervalTimeSecond; // TODO: syncIntervalTimeSecond���O������擾����K�v������
>>>>>>> Stashed changes:Assets/Scripts/Core/RTC/RTCObject.cs
        elapsedTime += Time.deltaTime;
        transform.position = Vector3.LerpUnclamped(currentPosition, recievedPosition, t);
        transform.rotation = Quaternion.LerpUnclamped(currentRotation, recievedRotation, t);
        GM.Msg("UpdateDistance", ownerId, recievedPosition);
        //if (objType != "human") return;
        //var speed = (currentPosition - transform.position).magnitude;
    }

    /// <summary>
<<<<<<< Updated upstream:Assets/Scripts/Core/Network/RTC/RTCObjectSync.cs
    /// TODO: 誰がこれを送信するかの決定が必要
=======
    /// TODO: �N������𑗐M���邩�̌��肪�K�v
>>>>>>> Stashed changes:Assets/Scripts/Core/RTC/RTCObject.cs
    /// </summary>
    /// <param name="path"></param>
    public async void SetObject(string path)
    {
        var avatarObj = GM.Msg<GameObject>("LoadAvatar", path);
        SetContent(avatarObj.transform);

<<<<<<< Updated upstream:Assets/Scripts/Core/Network/RTC/RTCObjectSync.cs
        // IPFSへのUpload
=======
        // IPFS�ւ�Upload
>>>>>>> Stashed changes:Assets/Scripts/Core/RTC/RTCObject.cs
        var cid = await GM.Msg<UniTask<string>>("UploadAvatar", path);
        this.cid = cid;
        GM.db.user.users[GM.db.rtc.id].cid = cid;

<<<<<<< Updated upstream:Assets/Scripts/Core/Network/RTC/RTCObjectSync.cs
        // 全体への通知
=======
        // �S�̂ւ̒ʒm
>>>>>>> Stashed changes:Assets/Scripts/Core/RTC/RTCObject.cs
        var sendData = new Dictionary<string, object>()
        {
            { "type", "change" },
            { "objId",  objId },
            { "objType",  objType },
            { "cid", cid },
        };
        GM.Msg("RPCSendAll", sendData);
    }

    /// <summary>
<<<<<<< Updated upstream:Assets/Scripts/Core/Network/RTC/RTCObjectSync.cs
    /// Avatar等を設定する
=======
    /// Avatar����ݒ肷��
>>>>>>> Stashed changes:Assets/Scripts/Core/RTC/RTCObject.cs
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
