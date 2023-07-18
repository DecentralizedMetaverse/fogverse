using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using DC;
using UnityEngine;

public class EventObject : Component, ExeEvent
{
    [SerializeField] public string hintText;
    public eYScript.exeType type = eYScript.exeType.interact;
    
    [SerializeField]
    public TextAsset luaScript;

    [System.NonSerialized]
    string _code;    // RunTime読み込み用
    public string code
    {
        get
        {
            if (luaScript == null)
            {
                return _code;
            }
            return luaScript.text;
        }
        set
        {
            if (luaScript == null)
            {
                _code = value;
            }
        }
    }

    void Start()
    {
        transform.tag = "Event";

        if(type == eYScript.exeType.first)
        {
            GM.MsgAsync("Run", code, gameObject).Forget();
        }
    }

    string ExeEvent.GetHint()
    {
        return hintText;
    }

    /// <summary>
    /// 決定キー実行
    /// </summary>
    /// <param name="vec"></param>
    async void ExeEvent.SubmitRun(Vector3 vec)
    {
        if (type != eYScript.exeType.interact) return;

        GM.Msg("Hint","", false);
        await GM.MsgAsync("Run", code, gameObject);
        GM.Msg("Hint", hintText, true);
    }

    /// <summary>
    /// 接触実行
    /// </summary>
    void ExeEvent.EnterRun()
    {
        if (type != eYScript.exeType.hit) return;

        GM.MsgAsync("Run", code).Forget();
    }
}
