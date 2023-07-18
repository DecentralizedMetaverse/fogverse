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
public class UI_Settings : MonoBehaviour
{
    [SerializeField] DB_Settings db;
    [SerializeField] UICache root;
    private TMP_InputField inputPass;
    private UI_ShowCloseFade ui;
    private string path;
    const string password = "NIOQ#afe09q23j";
    const string saltStr = "fwa903Gdf";

    byte[] salt;

    void Start()
    {
        var button = root.Get<Button>("SaveButton");
        button.onClick.AddListener(() => OnSavePassword());
        
        inputPass = root.Get<TMP_InputField>("PassInputField");
        
        ui = root.Get<UI_ShowCloseFade>();

        salt = System.Text.Encoding.UTF8.GetBytes(saltStr);
        path = $"{Application.persistentDataPath}/settings.dat";
        ReadPassword();
        inputPass.text = GM.password;

        GM.Add("ShowSettings", Show);
    }

    void Show()
    {
        ui.active = ui.active ? false : true;
    }


    private void OnSavePassword()
    {
        GM.password = inputPass.text;
        //var txt = Encoding.UTF8.GetBytes(inputPass.text);

        //// 16êiêîï∂éöóÒÇ…ïœä∑
        //var output = GM.Msg<byte[]>("Encrypt", txt, password, salt);
        
        //var passTxt = Encoding.UTF8.GetString(output);
        //File.WriteAllText(path, passTxt);
        File.WriteAllText(path, GM.password);
        
        ui.active = false;
    }

    void ReadPassword()
    {
        if (!File.Exists(path)) return;

        var txt = File.ReadAllText(path);
        //var byteTxt = Encoding.UTF8.GetBytes(txt);

        //// 16êiêîï∂éöóÒÇ…ïœä∑
        //var output = GM.Msg<byte[]>("Decrypt", byteTxt, password, salt);

        //GM.password = Encoding.UTF8.GetString(output);

        GM.password = txt;
    }
}
