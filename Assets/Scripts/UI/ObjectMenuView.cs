using DC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// オブジェクトにinteractすると開くメニュー
/// </summary>
public class ObjectMenuView : Menu
{
    Transform targetObj;

    /// <summary>
    /// オブジェクトを扱うため、引数でtransformを受け取る
    /// </summary>
    protected override void AddMsg()
    {
        GM.Add<Transform>($"Show{uiName}", Show);
        GM.Add($"Close{uiName}", Close);
    }

    void Show(Transform targetObj)
    {
        this.targetObj = targetObj;
        Show();
    }

    protected override void OnClick(int i)
    {
        GM.Msg(subMenu[i], targetObj);
        Close();
    }
}
