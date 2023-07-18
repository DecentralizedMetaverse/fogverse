using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class VersionView : MonoBehaviour
{
    TMP_Text versionText;

    void Start()
    {
        versionText = GetComponent<TMP_Text>();
        versionText.text = Application.version;
    }    
}
