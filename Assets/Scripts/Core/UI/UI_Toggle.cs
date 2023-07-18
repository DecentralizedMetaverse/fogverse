using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public abstract class UI_Toggle : MonoBehaviour
{
    [SerializeField] protected bool _active;
    public bool active
    {
        get
        {
            return _active;
        }
        set
        {
            if (value) Show();
            else Close();
        }
    }

    [System.NonSerialized] public CanvasGroup group;

    public delegate void OnCompleteDelegate();
    public delegate void OnComplete(bool a);
    /// <summary>
    /// アニメーション終了時に呼び出されるメソッド
    /// </summary>    
    public OnCompleteDelegate OnShow, OnClose;
    public OnComplete Finished;

    public abstract void Show();
    public abstract void Close();

    void Awake()
    {
        group = GetComponent<CanvasGroup>();

        //初期化　alphaの設定
        SetInit(_active);
    }

    protected void StartShow()
    {
        _active = true;
        SetInteractable(true);
        OnShow?.Invoke();
    }

    protected void StartClose()
    {
        _active = false;
        SetInteractable(false);
        OnClose?.Invoke();
    }

    protected void EndShow()
    {
        Finished?.Invoke(true);
    }

    protected void EndClose()
    {
        Finished?.Invoke(false);
    }

    /// <summary>
    /// UI初期状態
    /// </summary>
    /// <param name="b"></param>
    protected void SetInit(bool b)
    {
        group.alpha = b ? 1 : 0;
        group.interactable = b;
        group.blocksRaycasts = b;
    }

    /// <summary>
    /// 操作可能・不可能
    /// </summary>
    /// <param name="b"></param>
    protected void SetInteractable(bool b)
    {
        group.interactable = b;
        group.blocksRaycasts = b;
    }
}