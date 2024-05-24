using System.IO;
using DC;
using Teo.AutoReference;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Path = System.IO.Path;

/// <summary>
/// </summary>
public class ContentImportView : UIComponent
{
    private const string SaveDataKey = "ContentDirectory";
    private string contentPath = "";

    [SerializeField] private ButtonLabelView buttonPrefab;

    [Get, SerializeField] private UIEasingAnimationPosition animation;

    [GetInChildren, Name("DirectoryInputField"), SerializeField]
    private TMP_InputField input;

    [GetInChildren, Name("Content"), SerializeField]
    private Transform content;

    [GetInChildren, Name("SubmitButton"), SerializeField]
    private Button submitButton;

    private string[] files;
    private string currentPath;

    private void Start()
    {
        SaveData.I.TryGetValue(SaveDataKey, out string path);
        path ??= $"{Application.dataPath}/{GM.mng.contentPath}";
        currentPath = path;
        input.text = path;
        submitButton.onClick.AddListener(OnDirectoryChanged);
    }

    /// <summary>
    /// </summary>
    /// <param name="input"></param>
    private void OnDirectoryChanged()
    {
        currentPath = input.text;
        if (!Directory.Exists(currentPath)) return;

        SaveData.I.Set(SaveDataKey, currentPath);

        files = Directory.GetFiles(currentPath);
        content.DestroyChildren();

        var i = 0;
        foreach (var file in files)
        {
            var i1 = i;
            var button = Instantiate(buttonPrefab, content);
            button.gameObject.SetActive(true);
            button.Label.text = Path.GetFileName(file);
            button.Button.onClick.AddListener(() => OnSubmit(i1));
            i++;
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="i"></param>
    private void OnSubmit(int i)
    {
        // Copy file
        var source = files[i];
        var destination = $"{contentPath}/{Path.GetFileName(source)}";

        if (!File.Exists(destination))
        {
            File.Copy(source, destination);
        }

        GM.Msg("GenerateObj", destination);
        GM.Msg("RegisterObject"); // これを実行するタイミングに注意　全MetaFileが書き変わる
    }

    public override void Show()
    {
        base.Show();
        animation.Show();
        OnDirectoryChanged();
    }

    public override void Close()
    {
        base.Close();
        animation.Close();
    }
}
