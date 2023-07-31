using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using System;
using Cysharp.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Linq;
using DC;
/// <summary>
/// ゲーム管理クラス
/// TODO: Addメソッドの中身の共通部分を一つにまとめたい
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] DB db;
    [SerializeField] DB_GameManager _dbMng;
    [SerializeField] GameObject debugObj;
    [SerializeField] DB_FunctionList dBFunctionList;

#if UNITY_EDITOR
    void OnDestroy()
    {
        GM.SaveFunctionList(dBFunctionList);
    }
#endif

    private void Awake()
    {
        GM.pause = ePause.mode.GameStop;
        GM.mng = _dbMng;
        GM.db = db;
        GM.db.Init();

        GM.Init();
        //DOTween.Init(); // 最初に再生したときに画面が止まらないようにここで呼び出す
#if UNITY_EDITOR

        GM.mng.SetSceneName();
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled = false;
#endif
    }

    private void Start()
    {
        InputF.action.Debug.Quit.performed += OnQuit;
        InputF.action.Game.Cancel.performed += OnCancel;

        debugObj.SetActive(GM.mng.visiblePerformance);
    }

    void OnQuit(InputAction.CallbackContext contex)
    {
        GM.GameQuit();
    }

    void OnCancel(InputAction.CallbackContext context)
    {
        Debug.Log("OnCancel");

        GM.pause = GM.pause == ePause.mode.none ?
            ePause.mode.GameStop :
            ePause.mode.none;
    }

    [ContextMenu("Get Name")]
    public void Test(Delegate method)
    {
        var info = method.GetMethodInfo();
        var methodName = $"CS.{info.DeclaringType.FullName}.{info.Name}";
    }
}