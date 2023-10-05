using DC;
using UnityEngine;

[RequireComponent(typeof(RTCObjectSync))]
public class RTCUserObject : MonoBehaviour
{
    RTCObjectSync rtcObject;

    void Start()
    {
        rtcObject = GetComponent<RTCObjectSync>();
        GM.db.user.AddUser(rtcObject.ownerId, rtcObject.objId);
    }
}
