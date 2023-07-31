using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RTCObject))]
public class RTCAnimator : MonoBehaviour
{
    [SerializeField] RTCAnimatorStateData[] states = new RTCAnimatorStateData[]{
        new RTCAnimatorStateData(){ stateName = "Speed", type = Type.Float },
        new RTCAnimatorStateData(){ stateName = "Jump", type = Type.Bool },
        new RTCAnimatorStateData(){ stateName = "MotionSpeed", type = Type.Float },
    };
    RTCObject rtc;
    private float time;
    Dictionary<string, object> sendData = new();
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
            {Type.Int, (id) => { return rtc.animator.GetInteger(id); }},
            {Type.Float, (id) => { return rtc.animator.GetFloat(id); }},
            {Type.Bool, (id) => { return rtc.animator.GetBool(id); }},
        };
        
        setAnimStateValue = new()
        {
            {Type.Int, (id, value) => { rtc.animator.SetInteger(id, int.Parse(value.ToString())); } },
            {Type.Float, (id, value) => { rtc.animator.SetFloat(id, float.Parse( value.ToString())); }},
            {Type.Bool, (id, value) => { rtc.animator.SetBool(id, bool.Parse(value.ToString())); }},
        };

        rtc = GetComponent<RTCObject>();

        // ���M�f�[�^�̏�����
        sendData.Add("type", "anim");
        sendData.Add("objId", rtc.objId);
        sendData.Add("state", new Dictionary<string, object>());

        for (var i  = 0; i < states.Length; i++)
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
            // ��M�f�[�^�̔��f
            foreach(var state in states)
            {
                setAnimStateValue[state.type].DynamicInvoke(state.stateId, state.value);
            }
        }
    }

    /// <summary>
    /// Sending Animation State
    /// </summary>
    private void SendAnimState()
    {
        // ���M�Ԋu
        time += Time.deltaTime;
        if (time < GM.db.rtc.syncIntervalTimeSecond) return;
        time = 0;


        // Animator�̒l�𑗐M
        for (var i = 0; i < states.Length; i++)
        {
            var stateId = states[i].stateId;
            var stateIdStr = stateId.ToString();
            var stateType = states[i].type;

            stateData[stateIdStr] = getAnimStateValue[stateType].DynamicInvoke(stateId);
        }

        sendData["anim"] = stateData;

        GM.Msg("RTCSendAll", sendData);
    }

    /// <summary>
    /// Receive aimation data
    /// </summary>
    /// <param name="data"></param>
    public void ReceiveAnim(Dictionary<string, object> data)
    {
        if (rtc == null || rtc.animator == null)
        {
            Debug.LogWarning("Not found rtc or animator");
            return;
        }
        stateData = data["anim"].ToString().GetDict<string, object>();

        // Receive Animator State Value
        foreach(var (stateId, stateValue) in stateData)
        {
            var index = GetStateIndex(stateId);
            states[index].value = stateValue;
        }
    }

    /// <summary>
    /// Type��菉���l��Ԃ�
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
