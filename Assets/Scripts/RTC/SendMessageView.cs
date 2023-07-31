using DC;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SendMessageView : MonoBehaviour
{
    const int maxStringLen = 10000;
    [SerializeField] UI_Toggle uiToggle;
    public TMP_InputField address;
    public TMP_InputField input;
    public TMP_Text output;
    public TMP_InputField inputSyncInterval;
    public TMP_InputField inputAnimSyncInterval;
    string log;

    private void Start()
    {
        GM.Add<string>("GetLog", () =>
        {
            return log;
        });

        output.text = "";
        GM.Add<string>("AddOutput", (text) =>
        {
            log = $"{text}\n{log}";
            
            if (!uiToggle.active) return;

            var newText = $"{text}\n{output.text}";
            output.text = newText.TruncateText(maxStringLen);
        });

        inputSyncInterval.text = GM.db.rtc.syncIntervalTimeSecond.ToString();
        inputAnimSyncInterval.text = GM.db.rtc.syncAnimIntervalTimeSecond.ToString();
    }

    public void OnChangedSyncInterval()
    {
        if (!float.TryParse(inputSyncInterval.text, out var value))
        {
            inputSyncInterval.text = GM.db.rtc.syncIntervalTimeSecond.ToString();
            return;
        }

        GM.db.rtc.syncIntervalTimeSecond = value;
    }

    public void OnChangedAnimSyncInterval()
    {
        if(!float.TryParse(inputAnimSyncInterval.text, out var value))
        {
            inputAnimSyncInterval.text = GM.db.rtc.syncAnimIntervalTimeSecond.ToString();
            return;
        }

        GM.db.rtc.syncAnimIntervalTimeSecond = value;
    }
}
