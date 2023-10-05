using TMPro;
using UnityEngine;

public class RTCNameTag : MonoBehaviour
{
    [SerializeField] TMP_Text nameTag;

    void Start()
    {
        if (nameTag == null)
        {
            Debug.LogWarning("NameTag's TMP_Text is null");
            return;
        }

        nameTag.text = "無名";
    }

    public void SetName(string newName)
    {
        if (nameTag == null) return;

        nameTag.text = newName;
    }
}
