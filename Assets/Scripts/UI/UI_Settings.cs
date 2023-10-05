using AnKuchen.Map;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using DC;
using Cysharp.Threading.Tasks;

/// <summary>
/// 設定画面
/// </summary>
public class UI_Settings : MonoBehaviour
{
    [SerializeField] DB_Settings db;
    [SerializeField] UICache root;
    private TMP_InputField inputPass;
    private TMP_InputField inputNameTagField;
    private UI_ToggleFade ui;
    private string path;
    const string password = "NIOQ#afe09q23j";
    const string saltStr = "fwa903Gdf";

    byte[] salt;

    async void Start()
    {
        var button = root.Get<Button>("SaveButton");
        button.onClick.AddListener(() => OnSavePassword());
        
        inputPass = root.Get<TMP_InputField>("PassInputField");
        inputNameTagField = root.Get<TMP_InputField>("NameTagInputField");
        
        ui = root.Get<UI_ToggleFade>();

        // passwordのファイルを読み込む
        salt = System.Text.Encoding.UTF8.GetBytes(saltStr);
        path = $"{Application.persistentDataPath}/settings.dat";
        ReadPassword();
        inputPass.text = GM.password;

        GM.Add("ShowSettings", Show);

        // nametag取得
        //var result = GM.Msg<object>("GetSaveData", "nametag");
        //if (result == null) return;
        
        // nametagが取得できるまで待つ
        while(GM.db.rtc.selfObject == null)
        {
            await UniTask.Yield();
        }

        // nametagを設定
        inputNameTagField.text = GM.db.rtc.selfObject.nametag;
    }

    void Show()
    {
        ui.active = ui.active ? false : true;
    }

    /// <summary>
    /// passwordを保存する
    /// </summary>
    private void OnSavePassword()
    {
        GM.password = inputPass.text;
        GM.db.rtc.selfObject.nametag = inputNameTagField.text;
        GM.Msg("SetSaveData", "nametag", inputNameTagField.text);
        
        File.WriteAllText(path, GM.password);
        ui.active = false;
    }

    /// <summary>
    /// passwordを読み込む
    /// </summary>
    void ReadPassword()
    {
        // ファイルがなければ何もしない
        if (!File.Exists(path)) return;

        var txt = File.ReadAllText(path);        
        GM.password = txt;
    }
}
