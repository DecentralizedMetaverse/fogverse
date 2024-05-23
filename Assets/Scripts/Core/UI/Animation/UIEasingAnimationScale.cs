using DG.Tweening;
using UnityEngine;

public class UIEasingAnimationScale : UIComponent
{
    [SerializeField] private float animTimeSec = 0.3f;
    [SerializeField] private bool enableChangeAlpha = true;
    private RectTransform _rect;
    private CanvasGroup _canvasGroup;
    private Tweener _tweener;

    private void Start()
    {
        // RectTransformの設定
        _rect = GetComponent<RectTransform>();

        // Fadeの設定
        if (!enableChangeAlpha) return;
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        _canvasGroup.alpha = 0;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
    }

    public override void Show()
    {
        base.Show();
        _rect.localScale = Vector2.zero;
        _rect.DOScale(Vector3.one, animTimeSec).SetEase(Ease.OutQuint);
        if (enableChangeAlpha) ShowAlpha();
    }

    public override void Close()
    {
        base.Close();
        _rect.DOScale(Vector2.zero, animTimeSec).SetEase(Ease.InQuint);
        if (enableChangeAlpha) CloseAlpha();
    }

    private void ShowAlpha()
    {
        _tweener?.Kill();
        _tweener = _canvasGroup.DOFade(1, animTimeSec).SetEase(Ease.OutQuint);
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.interactable = true;
    }

    private void CloseAlpha()
    {
        _tweener?.Kill();
        _tweener = _canvasGroup.DOFade(0, animTimeSec).SetEase(Ease.InQuint).OnComplete(() =>
        {
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        });
    }
}
