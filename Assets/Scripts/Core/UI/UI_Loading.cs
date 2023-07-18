using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using DC;
using UnityEngine;

/// <summary>
/// [�S�̎g�p�\] ���[�f�B���O���
/// </summary>
[RequireComponent(typeof(UI_ShowCloseFade))]
public class UI_Loading : MonoBehaviour
{
    UI_ShowCloseFade ui;

    void Awake()
    {
        ui = GetComponent<UI_ShowCloseFade>();
    }

    void Start()
    {
        GM.Add("ShowLoading", ShowLoading);
        GM.Add("CloseLoading", CloseLoading);
    }

    /// <summary>
    /// ���[�h��ʂ�\������
    /// </summary>
    /// <returns></returns>
    async UniTask ShowLoading()
    {
        await ui.ShowAsync();
    }

    /// <summary>
    /// ���[�h��ʂ��\���ɂ���
    /// </summary>
    void CloseLoading()
    {
        ui.Close();
    }
}
