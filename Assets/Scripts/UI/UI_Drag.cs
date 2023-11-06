using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UI_Drag : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    Vector3 pos;

    void Start()
    {
        InputF.action.Game.Submit.canceled += OnCancel;
    }

    private void OnCancel(InputAction.CallbackContext obj)
    {
        pos = Vector3.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (pos != Vector3.zero)
        {
            transform.position += Input.mousePosition - pos;
        }
        pos = Input.mousePosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
    }
}
