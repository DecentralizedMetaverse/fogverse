using Cysharp.Threading.Tasks;
using DC;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// TODO: AFK時に表示を維持するように追加する
/// </summary>
public class ChatMessageView : MonoBehaviour
{
    const float fadeTimeSec = 0.5f;
    [SerializeField] Image userImage;
    [SerializeField] TMP_Text message;
    [SerializeField] TMP_Text userName;
    [SerializeField] Image contentImage;
    [SerializeField] Transform contentParent;
    [SerializeField] CanvasGroup group;

    public async void SetData(ChatMessageContent message)
    {
        userImage.sprite = message.userImage;
        this.message.text = message.content;
        userName.text = message.userName;

        if (!string.IsNullOrEmpty(message.avatarUrl))
        {
            userImage.sprite = await GM.Msg<UniTask<Sprite>>("LoadImage", message.avatarUrl);
        }

        if (message.contentImageUrl != null)
        {
            foreach (var url in message.contentImageUrl)
            {
                var image = await GM.Msg<UniTask<Sprite>>("LoadImage", url);
                if (image == null) continue;
                GameObject imageObj = new GameObject("Image");
                imageObj.transform.SetParent(contentParent);
                var contentImage = imageObj.AddComponent<Image>();
                contentImage.sprite = image;
            }
        }
    }

    public async UniTask DestroyDelay(float delayTimeSec)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delayTimeSec));

        await group.DOFade(0, fadeTimeSec);

        Destroy(gameObject);
    }
}
