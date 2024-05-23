using Teo.AutoReference;
using UnityEngine;
using UnityEngine.UI;

public class ButtonView : MonoBehaviour
{
    [GetInChildren, Name("IconImage")] public Image Icon;
    [GetInChildren] public Button Button;
    public UIComponent View;
}
