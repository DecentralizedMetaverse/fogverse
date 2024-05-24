using Teo.AutoReference;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonLabelView : MonoBehaviour
{
    [GetInChildren] public TMP_Text Label;
    [GetInChildren] public Button Button;
}
