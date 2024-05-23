using Teo.AutoReference;
using R3;
using UnityEngine;

public class MenuView : MonoBehaviour
{
    [GetInChildren] public ButtonView[] buttons;

    [Get, SerializeField] private UIEasingAnimationPosition animation;

    // ButtonのClickを監視する
    public Observable<ButtonView> OnClickAsObservable => _onClick;
    private Subject<ButtonView> _onClick = new();

    private void Start()
    {
        // 全てのButtonの入力を監視する
        foreach (var menu in buttons)
        {
            menu.Button.OnClickAsObservable().Subscribe(_ =>
            {
                _onClick.OnNext(menu);
            }).AddTo(this);
        }
    }

    public void Show()
    {
        animation.Show();
    }

    public void Close()
    {
        animation.Close();

        // 開いているメニューを閉じる
        foreach (var menu in buttons)
        {
            if (menu.View == null) continue;
            if (menu.View.IsShowing)
            {
                menu.View.Close();
            }
        }
    }
}
