using R3;
using UnityEngine;

public class InputController
{
    public static InputController I { get; private set; }
    public Observable<InputMode> OnChangedInputMode => _onChangedInputMode;
    private readonly Subject<InputMode> _onChangedInputMode = new();
    public InputMode Mode { get; private set; }
    public InputController()
    {
        I = this;
    }

    public void SetMode(InputMode inputMode)
    {
        Debug.Log($"[InputController] SetMode: {inputMode}");
        _onChangedInputMode.OnNext(inputMode);
    }
}
