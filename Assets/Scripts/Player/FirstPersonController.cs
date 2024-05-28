using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonController : MonoBehaviour
{
    [SerializeField] private Vector3 move;

    private void Start()
    {
        InputF.action.Game.Move.started += OnMove;
        InputF.action.Game.Move.performed += OnMove;
        InputF.action.Game.Move.canceled += OnMove;
      
    }

    private void OnMove(InputAction.CallbackContext obj)
    {

    }
}
