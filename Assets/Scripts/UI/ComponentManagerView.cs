using AnKuchen.KuchenList;
using AnKuchen.Map;
using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Teo.AutoReference;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ComponentをObjectに追加する
/// TODO: 破壊的な変更中　新しいUIシステムに変更中
/// </summary>
public class ComponentManagerView : MonoBehaviour
{
    [SerializeField] UICache root;
    [Get, SerializeField] private UIEasingAnimationScale animation;
    private string contentPath;
    private string[] files;
    
    Transform target;
    private string source;
    private TMP_InputField custonInputField;
    private UI_ToggleFade customWindow;

    void Start()
    {
        GM.Add<Transform>("ShowImportScript", Show);

        contentPath = Constants.ContentPath;

        // 各UIの取得
        var button = root.Get<Button>("SubmitButton");
        button.onClick.AddListener(() => OnOpenFolder());

        // custom画面
        var customSubmitButton = root.Get<Button>("CustomSubmitButton");
        customSubmitButton.onClick.AddListener(() => OnSubmitCustom());

        var closeButton = root.Get<Button>("CloseButton");
        closeButton.onClick.AddListener(() => OnClose());

        custonInputField = root.Get<TMP_InputField>("ComponentInputField");

        customWindow =root.Get<UI_ToggleFade>("Window2");
    }    

    /// <summary>
    /// UIの表示    
    /// </summary>
    /// <param name="target">Componentの追加先</param>
    void Show(Transform target)
    {
        animation.Show();

        this.target = target;

        string[] extensions = GM.Msg<string[]>("GetComponentExtensions");
        string pattern = "*" + string.Join("|*", extensions);

        files = Directory.GetFiles(contentPath, pattern);

        // using (var editor = ui.ItemList.Edit())
        // {
        //     // File一覧を表示
        //     int i = 0;
        //     foreach (string file in files)
        //     {
        //         var i1 = i;
        //         editor.Contents.Add(new UIFactory<FileButtonUiElements>(button =>
        //         {
        //             button.Text.text = Path.GetFileName(file);
        //             button.Button.onClick.AddListener(() => OnSubmit(i1));
        //
        //         }));
        //         i++;
        //     }
        // }
    }

    void OnOpenFolder()
    {
        System.Diagnostics.Process.Start(contentPath);
    }

    void OnSubmit(int i)
    {
        // ContentをWorldに生成する
        source = files[i];
        customWindow.active = true;
    }
    
    private void OnClose()
    {
        animation.Close();
        customWindow.active = false;
    }

    /// <summary>
    /// Custom Dataの決定
    /// </summary>
    void OnSubmitCustom()
    {
        animation.Close();
        customWindow.active = false;

        var custom = GM.Msg<Dictionary<string, object>>("ReadYamlText", custonInputField.text);

        GM.Msg("GenerateComponent", source, target, custom);
        
        // 生成したObjectを設定Fileに保存する
        // (注意: 全Objectの設定Fileを更新する)
        GM.Msg("RegisterObject");
        
        GM.Msg("ShortMessage", "Added Component");
    }
}
