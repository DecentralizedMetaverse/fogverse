using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SetNavigation : MonoBehaviour
{
    public enum layout { なし, 垂直, 水平, グリッド };
    [SerializeField] layout type = default;
    [SerializeField] int width = -1;
    [SerializeField] int height = -1;
    void Start()
    {
        if (type == layout.なし) return;
        Set();
    }
    public void Set()
    {
        SetNavigation(this.type, width, height);
    }
    public void SetNavigation(layout type, int width = -1, int height = -1)
    {
        Button[] buttons = new Button[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            buttons[i] = transform.GetChild(i).GetComponent<Button>();
        }
        if (buttons.Length == 0) return;
        if (type == layout.垂直) SetNavVertical(buttons);
        else if (type == layout.水平) SetNavHorizontal(buttons);
        else if (type == layout.グリッド) SetNavGrid(buttons, width, height);

    }
    //bt ターゲット, bt1 上, bt2 下
    Navigation nav;
    /// <summary>
    /// ボタンの遷移先を登録
    /// </summary>
    /// <param name="bt">ターゲット</param>
    /// <param name="bt1">上</param>
    /// <param name="bt2">下</param>
    void SetNavUD(Button bt, Button bt1, Button bt2)
    {
        nav = bt.navigation;
        nav.mode = Navigation.Mode.Explicit;
        nav.selectOnUp = bt1;
        nav.selectOnDown = bt2;
        bt.navigation = nav;
    }
    /// <summary>
    /// ボタンの遷移先を登録
    /// </summary>
    /// <param name="bt">ターゲット</param>
    /// <param name="bt1">左</param>
    /// <param name="bt2">右</param>
    void SetNavRL(Button bt, Button bt1, Button bt2)
    {
        nav = bt.navigation;
        nav.mode = Navigation.Mode.Explicit;
        nav.selectOnLeft = bt1;
        nav.selectOnRight = bt2;
        bt.navigation = nav;
    }
    /// <summary>
    /// 格子状に登録
    /// </summary>
    /// <param name="bt"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>    
    void SetNavGrid(Button[] bt, int width, int height)
    {
        //アクティブなボタンの要素数を取得
        int length = 0;
        for (; length < bt.Length; length++)
        {
            if (!bt[length].gameObject.activeSelf) break;
        }
        //遷移先の登録
        for (int i = 0; i < length; i++)
        {
            Navigation nav = bt[i].navigation;
            nav.mode = Navigation.Mode.Explicit;
            /*上*/
            if (i < width) nav.selectOnUp = bt[i + width * (height - 1)];
            else nav.selectOnUp = bt[i - width];
            /*下*/
            if (i + width >= length) nav.selectOnDown = bt[i % width];
            else nav.selectOnDown = bt[i + width];
            /*右*/
            if (i == length - 1) nav.selectOnRight = bt[0];
            else nav.selectOnRight = bt[i + 1];
            /*左*/
            if (i == 0) nav.selectOnLeft = bt[length - 1];
            else nav.selectOnLeft = bt[i - 1];
            /*ボタンに登録して終了*/
            bt[i].navigation = nav;
        }
    }
    public void SetNavVertical(Button[] bt)
    {
        if (bt.Length == 1) return;
        byte i = 1;
        for (; i < bt.Length - 1; i++)
        {
            if (!bt[i + 1].gameObject.activeSelf) break;
            SetNavUD(bt[i], bt[i - 1], bt[i + 1]);
        }
        SetNavUD(bt[0], bt[i], bt[1]);
        SetNavUD(bt[i], bt[i - 1], bt[0]);
    }
    void SetNavHorizontal(Button[] bt)
    {
        byte i = 1;
        for (; i < bt.Length - 1; i++)
        {
            if (!bt[i + 1].gameObject.activeSelf) break;
            SetNavRL(bt[i], bt[i - 1], bt[i + 1]);
        }
        SetNavRL(bt[0], bt[i], bt[1]);
        SetNavRL(bt[i], bt[i - 1], bt[0]);
    }
}
