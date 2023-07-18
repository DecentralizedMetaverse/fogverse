using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class UI_ToggleFade : UI_Toggle
{
    public float timeSec = 0;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();

        //初期化　alphaの設定
        SetInit(_active);
    }

    public override void Show()
    {
        StartShow();
        group.DOFade(1.0f, timeSec).OnComplete(() => EndShow());
    }

    public override void Close()
    {
        StartClose();
        group.DOFade(0, timeSec).OnComplete(() => EndClose());
    }

    public async UniTask ShowAsync()
    {
        StartShow();
        await group.DOFade(1.0f, timeSec);
        EndShow();
    }
}