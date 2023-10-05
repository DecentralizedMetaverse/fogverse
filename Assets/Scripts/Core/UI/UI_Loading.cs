using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using DC;
using UnityEngine;

/// <summary>
/// [全体使用可能] ローディング画面
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
    /// ロード画面を表示する
    /// </summary>
    /// <returns></returns>
    async UniTask ShowLoading()
    {
        await ui.ShowAsync();
    }

    /// <summary>
    /// ロード画面を非表示にする
    /// </summary>
    void CloseLoading()
    {
        ui.Close();
    }
}
