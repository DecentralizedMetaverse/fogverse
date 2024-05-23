public class MenuModel
{
    private readonly ButtonView[] _buttonViews;

    public MenuModel(ButtonView[] buttonViews)
    {
        _buttonViews = buttonViews;
    }

    public void OnClick(ButtonView subMenu)
    {
        if (subMenu.View == null) return;

        foreach (var menu in _buttonViews)
        {
            if (menu == subMenu) continue;
            if (menu.View == null) continue;

            if (menu.View.IsShowing)
            {
                menu.View.Close();
            }
        }

        // 開くか閉じるか決定
        var shouldShow = !subMenu.View.IsShowing;
        if (shouldShow)
        {
            subMenu.View.Show();
            subMenu.View.transform.SetAsLastSibling(); // 一番手前に表示
        }
        else
        {
            subMenu.View.Close();
        }
    }
}
