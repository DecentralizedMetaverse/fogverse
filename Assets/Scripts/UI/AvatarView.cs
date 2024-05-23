using AnKuchen.KuchenList;
using DC;
using System.IO;
using Teo.AutoReference;
using UnityEngine;
using UnityEngine.UI;

public class AvatarView : UIComponent
{
    [GetInChildren, Name("SubmitButton"), SerializeField] private Button submitButton;
    [Get, SerializeField] private UIEasingAnimationPosition animation;
    private ContentManagerUiElements ui;
    private string avatarPath;

    private void Start()
    {
        avatarPath = $"{Application.dataPath}/{GM.mng.avatarPath}";
        submitButton.onClick.AddListener(OnOpenFolder);
    }

    public override void Show()
    {
        base.Show();
        animation.Show();

        var files = Directory.GetFiles(avatarPath, "*.vrm");
        using var editor = ui.ItemList.Edit();
        foreach (var filePath in files)
        {
            editor.Contents.Add(new UIFactory<FileButtonUiElements>(button =>
            {
                button.Text.text = Path.GetFileName(filePath);
                button.Button.onClick.AddListener(() => OnClick(filePath));

            }));
        }
    }

    public override void Close()
    {
        base.Close();
        animation.Close();
    }

    private void OnClick(string filePath)
    {
        var id = GM.db.rtc.id;
        var objId = GM.db.rtc.syncObjectsByID[id][0];
        var obj = GM.db.rtc.syncObjects[objId];
        obj.SetObject(filePath);
    }

    private void OnOpenFolder()
    {
        System.Diagnostics.Process.Start(avatarPath);
    }
}
