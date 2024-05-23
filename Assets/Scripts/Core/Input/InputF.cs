using R3;
using UnityEngine;

public class InputF : MonoBehaviour
{
    public static @InputActions action;
    public InputController InputController = new InputController();

    private void Awake()
    {
        action = new @InputActions();
        action.Enable();
        InputController.OnChangedInputMode.Subscribe(SetOperation);
    }

    private void SetOperation(InputMode inputMode)
    {
        switch (inputMode)
        {
            case InputMode.GameAndUI:
                action.Game.Enable();
                action.UI.Disable();
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                break;
            case InputMode.UIOnly:
                action.UI.Enable();
                action.Game.Disable();
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;
            case InputMode.GameOnly:
                action.Game.Enable();
                action.UI.Disable();
                break;
        }
    }
}
