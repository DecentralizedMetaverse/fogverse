using Cysharp.Threading.Tasks;
using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���IObject�̍��W�Ǘ�
/// </summary>
public class RTCObject : MonoBehaviour
{
    // Object Data
    public bool isLocal = false;
    [System.NonSerialized] public string objId = "";
    public string ownerId = "";
    public string cid = "";
    public string objType = "human";    // TODO: �ʂ̏ꏊ�Őݒ肷�ׂ�

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
            GM.Msg("RTCSendAll", sendData);
        }
    }
    [SerializeField] string _nametag = "";
    RTCNameTag nameTag;

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
    // Humanoid
    public Animator animator;
    public RTCAnimator rtcAniamtor;

    // StarterAssets.StarterAssetsInputs starterAssets;

    // -----------------------------------
    private void Awake()
    {
        if (objType == "human")
        {
            // �l�^��Object�ł���΁AGeometry�ɐݒ肷��
            content = transform.Find("Geometry");
            TryGetComponent(out animator);
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
    /// Local�̏ꍇ�AData��ݒ肷��
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
    public void SetData(string objId, string cid, string objType, Vector3 position, Vector3 rotation)
    {
        this.objId = objId;
        this.cid = cid;
        this.objType = objType;
        isLocal = false;
        transform.position = position;
        transform.rotation = Quaternion.Euler(rotation);
        gameObject.name = $"{name}-{objId}";
    }

    /// <summary>
    /// Data�𑗐M���鏀��
    /// </summary>
    void PrepareSendLocationData()
    {
        locationData.Add("objId", "");
        locationData.Add("type", "location");
        locationData.Add("position", transform.position.ToSplitString());
        locationData.Add("rotation", transform.rotation.eulerAngles.ToSplitString());
    }

    /// <summary>
    /// ���W�𑗐M����
    /// </summary>
    void UpdataLocation()
    {
        // ���M�Ԋu�𒴂��Ă��Ȃ��ꍇ�́A���M���Ȃ�
        time += Time.deltaTime;
        if (time < GM.db.rtc.syncIntervalTimeSecond) return; 
        time = 0;

        // ���W���ς���Ă��Ȃ��ꍇ�́A���M���Ȃ�
        if (previousPosition == transform.position &&
            previousRotation == transform.rotation) return; 

        // ���W���قȂ�ꍇ�A���M����
        previousPosition = transform.position;
        transform.rotation = transform.rotation;

        locationData["position"] = transform.position.ToSplitString();
        locationData["rotation"] = transform.rotation.eulerAngles.ToSplitString();

        GM.Msg("RTCSendAll", locationData);
    }

    /// <summary>
    /// ���W����M����
    /// </summary>
    /// <param name="data"></param>
    public void ReceiveLocation(Dictionary<string, object> data)
    {
        recievedPosition = data["position"].ToString().ToVector3();
        recievedRotation = Quaternion.Euler(data["rotation"].ToString().ToVector3());

        currentPosition = transform.position;
        currentRotation = transform.rotation;
        elapsedTime = 0;
    }

    /// <summary>
    /// ���W�̕��
    /// </summary>
    void InterpolationLocation()
    {
        var t = elapsedTime / GM.db.rtc.syncIntervalTimeSecond;
        elapsedTime += Time.deltaTime;
        transform.position = Vector3.LerpUnclamped(currentPosition, recievedPosition, t);
        transform.rotation = Quaternion.LerpUnclamped(currentRotation, recievedRotation, t);

        if (objType != "human") return;
        var speed = (currentPosition - transform.position).magnitude;
    }

    /// <summary>
    /// TODO: �N������𑗐M���邩�̌��肪�K�v
    /// </summary>
    /// <param name="path"></param>
    public async void SetObject(string path)
    {
        var avatarObj = GM.Msg<GameObject>("LoadAvatar", path);
        SetContent(avatarObj.transform);

        // IPFS�ւ�Upload
        var cid = await GM.Msg<UniTask<string>>("UploadAvatar", path);
        this.cid = cid;
        GM.db.user.users[GM.db.rtc.id].cid = cid;

        // �S�̂ւ̒ʒm
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
    /// Avatar����ݒ肷��
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
