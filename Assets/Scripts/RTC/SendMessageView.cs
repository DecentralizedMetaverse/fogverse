using DC;
using System.Collections;
using System.Collections.Generic;
using System.Net;
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

        GM.Msg("AddOutput", $"Device Unique Identifier: {UnityEngine.SystemInfo.deviceUniqueIdentifier}");
        GM.Msg("AddOutput", $"Processor Type: {UnityEngine.SystemInfo.processorType}");
        GM.Msg("AddOutput", $"Graphics Device Name: {UnityEngine.SystemInfo.graphicsDeviceName}");
        GM.Msg("AddOutput", $"Processor Frequency: {UnityEngine.SystemInfo.processorFrequency} MHz");
        GM.Msg("AddOutput", $"Processor Count: {UnityEngine.SystemInfo.processorCount}");
        GM.Msg("AddOutput", $"Graphics Device Type: {UnityEngine.SystemInfo.graphicsDeviceType}");
        GM.Msg("AddOutput", $"Device Name: {UnityEngine.SystemInfo.deviceName}");
        GM.Msg("AddOutput", $"Device Type: {UnityEngine.SystemInfo.deviceType}");
        GM.Msg("AddOutput", $"Graphics Memory Size: {UnityEngine.SystemInfo.graphicsMemorySize} MB");
        GM.Msg("AddOutput", $"Battery Level: {UnityEngine.SystemInfo.batteryLevel * 100}%"); // Battery level is a value between 0 and 1
        GM.Msg("AddOutput", $"Battery Status: {UnityEngine.SystemInfo.batteryStatus}");

        string hostname = Dns.GetHostName();
        IPAddress[] ipAddresses = Dns.GetHostAddresses(hostname);
        foreach (IPAddress ipAddress in ipAddresses)
        {
            GM.Msg("AddOutput", $"IP Address: {ipAddress.ToString()}");
        }
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
