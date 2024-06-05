using DC;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Objectを操作する
/// </summary>
public class InteractObject : MonoBehaviour
{
    private RaycastHit hit;

    void Start()
    {
        InputF.action.Game.Submit.performed += OnSubmit;
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        if (InputController.I.Mode != InputMode.UIOnly) return;

        if (!InputF.action.Game.Sprint.IsPressed()) return;

        var pos = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(pos, out hit, 5000))
        {
            var root = hit.transform;
            GM.Msg("ShowObjectMenu", root);
        }
    }
}
