using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Reflection;
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

    private SaveData saveData;
    private PersonViewController personViewController = new PersonViewController();

    void OnDestroy()
    {
        GM.db.End();

#if UNITY_EDITOR
        GM.SaveFunctionList(dBFunctionList);
#endif
    }

    private void Awake()
    {
        saveData = new SaveData();
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
        InputController.I.SetMode(InputMode.GameAndUI);

        InputF.action.Debug.Quit.performed += OnQuit;

        debugObj.SetActive(GM.mng.visiblePerformance);

        GM.db.Start();
    }


    void OnQuit(InputAction.CallbackContext contex)
    {
        GM.GameQuit();
    }

    [ContextMenu("Get Name")]
    public void Test(Delegate method)
    {
        var info = method.GetMethodInfo();
        var methodName = $"CS.{info.DeclaringType.FullName}.{info.Name}";
    }
}
