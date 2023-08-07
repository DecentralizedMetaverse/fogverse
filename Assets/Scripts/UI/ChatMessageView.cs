using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TODO: AFKéûÇ…ï\é¶Çà€éùÇ∑ÇÈÇÊÇ§Ç…í«â¡Ç∑ÇÈ
/// </summary>
public class ChatMessageView : MonoBehaviour
{
    const float fadeTimeSec = 0.5f;
    [SerializeField] Image userImage;
    [SerializeField] TMP_Text message;
    [SerializeField] TMP_Text userName;
    [SerializeField] CanvasGroup group;

    public void SetData(ChatMessageContent message)
    {
        userImage.sprite = message.userImage;
        this.message.text = message.content;
        userName.text = message.userName;
    }

    public async UniTask DestroyDelay(float delayTimeSec)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delayTimeSec));

        await group.DOFade(0, fadeTimeSec);

        Destroy(gameObject);
    }
}
