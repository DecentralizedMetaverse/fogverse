using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ePause
{
    /// <summary>
    /// none        : 何もない       
    /// GameStop    : ゲーム画面が停止する UIのみ操作可能
    /// UIStop      : メニュー画面などが表示できなくなる
    /// </summary>
    public enum mode
    {
        none, UIStop, GameStop
    }
}
