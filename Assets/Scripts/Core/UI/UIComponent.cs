using UnityEngine;

public abstract class UIComponent : MonoBehaviour
{
    public bool IsShowing { get; protected set; }

    public virtual void Show()
    {
        IsShowing = true;
    }

    public virtual void Close()
    {
        IsShowing = false;
    }
}
