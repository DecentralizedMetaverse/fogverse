using DC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Objectを選択して、Pipeを設定する
/// TODO: 選択モードに追加する
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

        if (GM.pause != ePause.mode.GameStop) return;

        var pos = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(pos, out RaycastHit hit, 5000)) return;

        // 二つ目のObjectを取得
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
    /// 接続を確定する
    /// </summary>
    void SetConnect()
    {
        sourcePipeObj.Add(targetPipeObj);
        GM.Msg("ShortMessage", "Connection Success");
    }

    /// <summary>
    /// 終了処理
    /// </summary>
    void FinishSearchingPipeObject()
    {
        sourcePipeObj = null;
        targetPipeObj = null;
        isEnable = false;
    }
}
