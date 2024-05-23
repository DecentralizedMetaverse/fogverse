using DG.Tweening;
using UnityEngine;

public class UIEasingAnimationPosition : UIComponent
{
    [SerializeField] private float animTimeSec = 0.3f;
    [SerializeField] private Vector2 openPositionDelta;
    [SerializeField] private bool enableChangeAlpha = true;

    private RectTransform _rect;
    private Vector2 _originalPos;
    private Vector2 _closedPos;
    private CanvasGroup _canvasGroup;
    private Tweener _tweener;

    private void Start()
    {
        // RectTransformの設定
        _rect = GetComponent<RectTransform>();
        _originalPos = _rect.anchoredPosition;
        _closedPos = _originalPos + openPositionDelta;

        // Fadeの設定
        if (!enableChangeAlpha) return;
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    public override void Show()
    {
        base.Show();
        _rect.anchoredPosition = _closedPos;
        _rect.DOAnchorPos(_originalPos, animTimeSec).SetEase(Ease.OutQuint);
        if (enableChangeAlpha) ShowAlpha();
    }

    public override void Close()
    {
        base.Close();
        _rect.DOAnchorPos(_closedPos, animTimeSec).SetEase(Ease.InQuint);
        if (enableChangeAlpha) CloseAlpha();
    }

    private void ShowAlpha()
    {
        _tweener?.Kill();
        _tweener = _canvasGroup.DOFade(1, animTimeSec).SetEase(Ease.OutQuint);
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    private void CloseAlpha()
    {
        _tweener?.Kill();
        _tweener = _canvasGroup.DOFade(0, animTimeSec).SetEase(Ease.InQuint).OnComplete(() =>
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        });
    }
}
