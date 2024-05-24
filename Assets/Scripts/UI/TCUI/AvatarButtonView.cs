using Teo.AutoReference;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AvatarButtonView : MonoBehaviour
{
    [Get, SerializeField] public Button button;
    [GetInChildren, SerializeField] public Image avatarImage;
    [GetInChildren, SerializeField] public TMP_Text avatarName;
}
