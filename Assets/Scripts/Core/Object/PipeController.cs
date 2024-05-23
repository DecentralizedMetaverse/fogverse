using DC;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Object��I�����āAPipe��ݒ肷��
/// TODO: �I�����[�h�ɒǉ�����
/// </summary>
public class PipeController : MonoBehaviour
{
    ObjectPipe sourcePipeObj;
    IPipe targetPipeObj;

    bool isEnable;

    void Start()
    {
        InputF.action.Game.Submit.performed += OnSubmit;
        InputF.action.Game.Cancel.performed += OnCancel;
        GM.Add<Transform>("EnableObjectSelection", (targetObj) =>
        {
            if (!targetObj.TryGetComponent(out ObjectPipe objPipe))
            {
                objPipe = targetObj.gameObject.AddComponent<ObjectPipe>();
            }

            sourcePipeObj = objPipe;
            isEnable = true;
            GM.Msg("ShortMessage", "Please select a second object");
        });
        GM.Add("DisableObjectSelection", () => { isEnable = false; });
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        if (!isEnable) return;

        if (InputController.I.Mode != InputMode.UIOnly) return;

        var pos = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(pos, out RaycastHit hit, 5000)) return;

        // ��ڂ�Object���擾
        if (!hit.transform.TryGetComponent(out IPipe targetObj))
        {
            GM.Msg("ShortMessage", "This object is not supported");
            return;
        }

        targetPipeObj = targetObj;
        SetConnect();
        FinishSearchingPipeObject();
    }

    private void OnCancel(InputAction.CallbackContext obj)
    {
        if (!isEnable) return;
        FinishSearchingPipeObject();
        GM.Msg("ShortMessage", "Canceled");
    }

    /// <summary>
    /// �ڑ����m�肷��
    /// </summary>
    void SetConnect()
    {
        sourcePipeObj.Add(targetPipeObj);
        GM.Msg("ShortMessage", "Connection Success");
    }

    /// <summary>
    /// �I������
    /// </summary>
    void FinishSearchingPipeObject()
    {
        sourcePipeObj = null;
        targetPipeObj = null;
        isEnable = false;
    }
}
