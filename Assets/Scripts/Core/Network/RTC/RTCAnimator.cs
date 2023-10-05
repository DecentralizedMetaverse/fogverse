using System;
using System.Collections.Generic;
using DC;
using UnityEngine;

[RequireComponent(typeof(RTCObjectSync))]
public class RTCAnimator : MonoBehaviour
{
    [SerializeField]
    RTCAnimatorStateData[] states = new RTCAnimatorStateData[]{
        new RTCAnimatorStateData(){ stateName = "Speed", type = Type.Float },
        new RTCAnimatorStateData(){ stateName = "Jump", type = Type.Bool },
        new RTCAnimatorStateData(){ stateName = "MotionSpeed", type = Type.Float },
    };
    RTCObjectSync rtc;
    private float time;
    P_Animation sendData = new();
    Dictionary<string, object> stateData = new();
    Dictionary<Type, Func<int, object>> getAnimStateValue;
    Dictionary<Type, Action<int, object>> setAnimStateValue;

    Dictionary<string, int> stateIndex = new();
    int GetStateIndex(string stateId)
    {
        return stateIndex[stateId];
    }

    public enum Type
    {
        Int, Float, Bool, Trigger
    }

    void Start()
    {
        Init();
    }

    public void Init()
    {
        getAnimStateValue = new()
        {
            {Type.Int, (id) => { return rtc.animator ?.GetInteger(id); }},
            {Type.Float, (id) => { return rtc.animator ?.GetFloat(id); }},
            {Type.Bool, (id) => { return rtc.animator?.GetBool(id); }},
        };

        setAnimStateValue = new()
        {
            {Type.Int, (id, value) => { rtc.animator?.SetInteger(id, int.Parse(value.ToString())); } },
            {Type.Float, (id, value) => {
                try
                {
                    rtc.animator ?.SetFloat(id, float.Parse(value.ToString()));
                }
                catch(Exception e)
                {
                    Debug.Log(e);
                }
            }},
            {Type.Bool, (id, value) => { rtc.animator ?.SetBool(id, bool.Parse(value.ToString())); }},
        };

        rtc = GetComponent<RTCObjectSync>();

        // 送信データの初期化
        //sendData.Add("type", "anim");
        sendData.objId = rtc.objId;
        // sendData.Add("objId", rtc.objId);
        // sendData.Add("state", new Dictionary<string, object>());
        // sendData.state

        for (var i = 0; i < states.Length; i++)
        {
            states[i].stateId = Animator.StringToHash(states[i].stateName);
            stateData.Add(states[i].stateId.ToString(), InitType(states[i].type));
            stateIndex.Add(states[i].stateId.ToString(), i);
        }
    }

    private void Update()
    {
        if (rtc.isLocal)
        {
            SendAnimState();
        }
        else
        {
            // 受信データの反映
            foreach (var state in states)
            {
                if (state.value == null) continue;
                setAnimStateValue[state.type].DynamicInvoke(state.stateId, state.value);
            }
        }
    }

    /// <summary>
    /// Sending Animation State
    /// </summary>
    private void SendAnimState()
    {
        // 送信間隔
        time += Time.deltaTime;
        if (time < GM.db.rtc.syncIntervalTimeSecond) return;
        time = 0;


        // Animatorの値を取得
        for (var i = 0; i < states.Length; i++)
        {
            var stateId = states[i].stateId;
            var stateIdStr = stateId.ToString();
            var stateType = states[i].type;

            stateData[stateIdStr] = getAnimStateValue[stateType].DynamicInvoke(stateId);
        }

        // sendData["state"] = stateData;
        sendData.state = stateData.GetString();

        // いつでも自分の情報を送れるように準備しておく
        GM.Msg("SetSelfAnimationData", sendData);
    }

    /// <summary>
    /// Receive aimation data
    /// </summary>
    /// <param name="data"></param>
    public void ReceiveAnim(P_Animation data)
    {
        if (rtc == null || rtc.animator == null)
        {
            Debug.LogWarning("Not found rtc or animator");
            return;
        }
        stateData = data.state.GetDict<string, object>();

        // Receive Animator State Value
        foreach (var (stateId, stateValue) in stateData)
        {
            var index = GetStateIndex(stateId);
            states[index].value = stateValue;
        }
    }

    /// <summary>
    /// Typeより初期値を返す
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    object InitType(Type type)
    {
        switch (type)
        {
            default:
                return null;
            case Type.Int:
                return 0;
            case Type.Float:
                return 0f;
            case Type.Bool:
                return false;
            case Type.Trigger:
                return false;
        }
    }
}

[System.Serializable]
public class RTCAnimatorStateData
{
    public string stateName;
    public int stateId;
    public RTCAnimator.Type type;
    [System.NonSerialized] public object value;
}
