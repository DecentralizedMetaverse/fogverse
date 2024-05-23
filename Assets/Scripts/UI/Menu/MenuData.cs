using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

[Serializable]
public class MenuData
{
    public ButtonView ButtonView;
    public UIComponent View;
    public Sprite Icon;

    /// <summary>
    /// 実行タイミングはStartではなく画面表示時
    /// Runtimeでの言語切り替えに対応できるようにするため
    /// </summary>
    public async UniTask Init()
    {
        ButtonView.Icon.sprite = Icon;
    }
}
