using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using DC;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTrigger : MonoBehaviour
{
    protected List<GameObject> runnableObjs = new List<GameObject>();
    protected List<ExeEvent> exeEvents = new List<ExeEvent>();


    private void Start()
    {
        InputF.action.Game.Submit.performed += OnSubmit;
    }

    void Update()
    {
        CheckNullRunnableObj();
    }

    private void CheckNullRunnableObj()
    {
        if (runnableObjs.Count == 0) return;
        if (runnableObjs[0] != null) return;

        //イベント実行によりゲームObjectが消えた場合
        runnableObjs.RemoveAt(0);
        exeEvents.RemoveAt(0);

        ShowHintHandler();
    }

    /// <summary>
    /// 決定キーを押したときの処理
    /// </summary>
    /// <param name="contex"></param>
    void OnSubmit(InputAction.CallbackContext contex)
    {
        if (GM.pause == ePause.mode.GameStop) return;
        if (runnableObjs.Count == 0) return;

        exeEvents[0].SubmitRun(transform.position);
    }

    protected void AddRunnableObj(GameObject obj)
    {
        var exe = obj.GetComponent<ExeEvent>();
        EnterRun(exe).Forget();
        runnableObjs.Add(obj);
        exeEvents.Add(exe);
    }

    async UniTask EnterRun(ExeEvent exe)
    {
        await UniTask.WaitWhile(() => (GM.pause == ePause.mode.GameStop));
        exe.EnterRun();
    }

    protected void RemoveRunnableObj(GameObject obj)
    {
        runnableObjs.Remove(obj);
        exeEvents.Remove(obj.GetComponent<ExeEvent>());
    }

    protected void ShowHintHandler()
    {
        if (runnableObjs.Count == 0)
        {
            GM.Msg("Hint", "", false);
        }
        else
        {
            GM.Msg("Hint", exeEvents[0].GetHint(), true);
        }
    }
}
