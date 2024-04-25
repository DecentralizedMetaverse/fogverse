using DC;
using MistNet;
using Teo.AutoReference;
using TMPro;
using UnityEngine;

public class RTCNameTag : MonoBehaviour
{
    [SerializeField] TMP_Text nameTag;
    [Get, SerializeField] private MistSyncObject syncObject;

    [MistSync(OnChanged = nameof(OnChangedName))]
    private string Name { get; set; }

    private void Start()
    {
        if (!syncObject.IsOwner) return;

        nameTag.text = "";
        var nameData =GM.Msg<object>("GetSaveData", "nametag");
        Name = nameData == null ? "" : nameData.ToString();
    }

    public void OnChangedName()
    {
        nameTag.text = Name;
    }
}
