using DC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �I�u�W�F�N�g��interact����ƊJ�����j���[
/// </summary>
public class ObjectMenuView : Menu
{
    Transform targetObj;

    /// <summary>
    /// �I�u�W�F�N�g���������߁A������transform���󂯎��
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
