using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DC;
using TC;
using Teo.AutoReference;

/// <summary>
/// 設定画面
/// </summary>
public class SettingsView : UIComponent
{
    [GetInChildren, Name("PassInputField"), SerializeField] private TMP_InputField inputPass;
    [GetInChildren, Name("NameTagInputField"), SerializeField] private TMP_InputField inputNameTagField;
    [GetInChildren, Name("SaveButton"), SerializeField] private Button saveButton;
    [Get, SerializeField] private UIEasingAnimationPosition animation;

    private void Start()
    {
        saveButton.onClick.AddListener(OnSavePassword);
        if (SaveData.I.TryGetValue("password", out string password))
        {
            GM.password = password;
            inputPass.text = password;
            Message.Send("FwSetPassword", password);
        }
    }

    public override void Show()
    {
        base.Show();
        animation.Show();
    }

    public override void Close()
    {
        base.Close();
        animation.Close();
    }

    /// <summary>
    /// passwordを保存する
    /// </summary>
    private void OnSavePassword()
    {
        GM.password = inputPass.text;
        SaveData.I.Set("password", GM.password);
        Message.Send("FwSetPassword", GM.password);
        Close();
    }
}
