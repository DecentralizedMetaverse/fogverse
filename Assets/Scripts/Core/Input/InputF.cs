using DC;
using R3;
using UnityEngine;

public class InputF : MonoBehaviour
{
    public static @InputActions action;
    private readonly InputController inputController = new();

    private void Awake()
    {
        action = new @InputActions();
        action.Enable();
        inputController.OnChangedInputMode.Subscribe(SetOperation).AddTo(this);
    }

    private static void SetOperation(InputMode inputMode)
    {
        Debug.Log($"[InputController] SetOperation SetMode: {inputMode}");
        switch (inputMode)
        {
            case InputMode.GameAndUI:
                action.Game.Enable();
                action.UI.Disable();
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                GM.Msg("SetPauseCamera", false);
                GM.Msg("SetEnableMove", true);
                break;
            case InputMode.UIOnly:
                action.Game.Disable();
                action.UI.Enable();
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                GM.Msg("SetPauseCamera", true);
                GM.Msg("SetEnableMove", false);
                break;
            case InputMode.GameOnly:
                action.Game.Enable();
                action.UI.Disable();
                GM.Msg("SetPauseCamera", false);
                GM.Msg("SetEnableMove", true);
                break;
        }
    }
}
