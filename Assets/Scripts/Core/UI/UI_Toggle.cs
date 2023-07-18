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
    /// �A�j���[�V�����I�����ɌĂяo����郁�\�b�h
    /// </summary>    
    public OnCompleteDelegate OnShow, OnClose;
    public OnComplete Finished;

    public abstract void Show();
    public abstract void Close();

    void Awake()
    {
        group = GetComponent<CanvasGroup>();

        //�������@alpha�̐ݒ�
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
    /// UI�������
    /// </summary>
    /// <param name="b"></param>
    protected void SetInit(bool b)
    {
        group.alpha = b ? 1 : 0;
        group.interactable = b;
        group.blocksRaycasts = b;
    }

    /// <summary>
    /// ����\�E�s�\
    /// </summary>
    /// <param name="b"></param>
    protected void SetInteractable(bool b)
    {
        group.interactable = b;
        group.blocksRaycasts = b;
    }
}