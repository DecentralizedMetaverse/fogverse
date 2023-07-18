using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using DC;
using UnityEngine;
using UnityEngine.UI;

public class ShortMessage : MonoBehaviour
{
    [SerializeField] RectTransform rect;
    [SerializeField] Text text;
    [SerializeField] CanvasGroup group;
    const float timeSec = 1.0f;
    
    void Start()
    {
        GM.Add<string, UniTask>("ShortMessage", Show, true);
        rect.gameObject.SetActive(false);
    }

    public async UniTask Show(string message)
    {
        text.text = message;
        
        rect.gameObject.SetActive(true);
        await rect.DOAnchorPosY(220f, 0);
        group.DOFade(1, 0.5f);
        await rect.DOAnchorPosY(200f, 0.5f);
        
        await UniTask.Delay(TimeSpan.FromSeconds(timeSec));
        
        group.DOFade(0, 0.5f);
        await rect.DOAnchorPosY(180f, 0.5f);
        rect.gameObject.SetActive(false);
    }
}
