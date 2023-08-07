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
/// �ݒ���
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

        // password�̃t�@�C����ǂݍ���
        salt = System.Text.Encoding.UTF8.GetBytes(saltStr);
        path = $"{Application.persistentDataPath}/settings.dat";
        ReadPassword();
        inputPass.text = GM.password;

        GM.Add("ShowSettings", Show);

        // nametag�擾
        //var result = GM.Msg<object>("GetSaveData", "nametag");
        //if (result == null) return;
        
        // nametag���擾�ł���܂ő҂�
        while(GM.db.rtc.selfObject == null)
        {
            await UniTask.Yield();
        }

        // nametag��ݒ�
        inputNameTagField.text = GM.db.rtc.selfObject.nametag;
    }

    void Show()
    {
        ui.active = ui.active ? false : true;
    }

    /// <summary>
    /// password��ۑ�����
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
    /// password��ǂݍ���
    /// </summary>
    void ReadPassword()
    {
        // �t�@�C�����Ȃ���Ή������Ȃ�
        if (!File.Exists(path)) return;

        var txt = File.ReadAllText(path);        
        GM.password = txt;
    }
}
