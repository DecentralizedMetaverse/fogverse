using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private static readonly float HoverScaleTimeSec = 0.15f;
    private static readonly float PressScaleTimeSec = 0.1f;
    private static readonly float ScaleMultiplier = 1.1f;

    [SerializeField] private CanvasGroup _outlineCanvasGroup;

    private Vector3 _originalScale;
    private Sequence _pressAnimationSequence;
    private bool _isSelecting;

    private void Start()
    {
        _originalScale = transform.localScale;
        _pressAnimationSequence = DOTween.Sequence()
            .Append(transform.DOScale(_originalScale, PressScaleTimeSec))
            .Append(transform.DOScale(_originalScale * ScaleMultiplier, PressScaleTimeSec))
            .SetAutoKill(false)
            .Pause();
        if (_outlineCanvasGroup) _outlineCanvasGroup.alpha = 0;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnRelease();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnPress();
    }

    private void OnHover()
    {
        transform.DOScale(_originalScale * ScaleMultiplier, HoverScaleTimeSec);
        if (_outlineCanvasGroup) _outlineCanvasGroup.DOFade(1, HoverScaleTimeSec);
    }

    private void OnRelease()
    {
        _isSelecting = false;
        transform.DOScale(_originalScale, HoverScaleTimeSec);
        if (_outlineCanvasGroup) _outlineCanvasGroup.DOFade(0, HoverScaleTimeSec);
    }

    private void OnPress()
    {
        _isSelecting = true;
        _pressAnimationSequence.Restart();
    }
}
