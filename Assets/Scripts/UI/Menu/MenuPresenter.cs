using Teo.AutoReference;
using R3;
using UnityEngine;

public class MenuPresenter : UIComponent
{
    [Get, SerializeField] private MenuView view;
    private MenuModel _model;

    private void Start()
    {
        _model = new MenuModel(view.buttons);
        view.OnClickAsObservable.Subscribe(_model.OnClick).AddTo(this);
    }

    public override void Show()
    {
        base.Show();
        view.Show();
    }

    public override void Close()
    {
        base.Close();
        view.Close();
    }
}
