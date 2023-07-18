using DC;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RTCNameTag : MonoBehaviour
{
    [SerializeField] TMP_Text nameTag;
    
    public void SetName(string newName)
    {
        nameTag.text = newName;
    }
}
