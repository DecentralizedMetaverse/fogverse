using DG.Tweening;
using Teo.AutoReference;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIFadeAnimation : UIComponent
{
    [Get, SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float animTimeSec = 0.3f;
    private Tweener _tweener;

    private void Start()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public override void Show()
    {
        base.Show();
        _tweener?.Kill();
        _tweener = canvasGroup.DOFade(1, animTimeSec).SetEase(Ease.OutQuint);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public override void Close()
    {
        base.Close();
        _tweener?.Kill();
        _tweener = canvasGroup.DOFade(0, animTimeSec).SetEase(Ease.InQuint).OnComplete(() =>
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        });
    }
}
