using AnKuchen.KuchenLayout;
using AnKuchen.Map;
using Cysharp.Threading.Tasks;
using DG.Tweening.Plugins.Core.PathCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using DC;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Path = System.IO.Path;
using AnKuchen.KuchenList;

/// <summary>
/// Fileをコピーする
/// </summary>
public class ContentImportView : MonoBehaviour
{
    [SerializeField] UICache root;
    ContentImporterUiElements ui;
    private TMP_InputField input;
    string contentPath = "";

    string[] files;
    string currentPath;

    void Start()
    {
        contentPath = $"{Application.dataPath}/{GM.mng.contentPath}";
        currentPath = contentPath;

        ui = new ContentImporterUiElements(root);
        input = root.Get<TMP_InputField>("DirectoryInputField");
        input.text = currentPath;

        var button = root.Get<Button>("SubmitButton");
        button.onClick.AddListener(() => OnDirectoryChanged());

        var show = root.Get<UI_ShowCloseFade>();
        GM.Add("ShowContentImporter", () =>
        {
            show.active = show.active ? false : true;
        }, true);

    }

    /// <summary>
    /// ディレクトリー内のFile一覧を取得する
    /// </summary>
    /// <param name="input"></param>
    void OnDirectoryChanged()
    {
        currentPath = input.text;
        if (!Directory.Exists(currentPath)) return;

        // ディレクトリ内のFile一覧を取得
        files = Directory.GetFiles(currentPath);
        using (var editor = ui.ItemList.Edit())
        {
            //editor.con
            // File一覧を表示
            int i = 0;
            foreach (string file in files)
            {
                var i1 = i;

                editor.Contents.Add(new UIFactory<FileButton46UiElements>(button =>
                {
                    button.Text.text = Path.GetFileName(file);
                    button.Button.onClick.AddListener(() => OnSubmit(i1));
                }
                ));
                i++;
            }
        }


    }

    /// <summary>
    /// File選択時の処理
    /// </summary>
    /// <param name="i"></param>
    void OnSubmit(int i)
    {
        // Copy file
        string source = files[i];
        string destination = $"{contentPath}/{Path.GetFileName(source)}";

        if (File.Exists(destination)) return;

        File.Copy(source, destination);
    }
}

public class ContentImporterUiElements : IMappedObject
{
    public IMapper Mapper { get; private set; }
    public GameObject Root { get; private set; }
    public Button SubmitButton { get; private set; }
    public Button FileButton { get; private set; }
    public VerticalList<FileButton46UiElements> ItemList { get; private set; }


    public ContentImporterUiElements() { }
    public ContentImporterUiElements(IMapper mapper) { Initialize(mapper); }

    public void Initialize(IMapper mapper)
    {
        Mapper = mapper;
        Root = mapper.Get();
        SubmitButton = mapper.Get<Button>("SubmitButton");
        FileButton = mapper.Get<Button>("FileButton");
        ItemList = new VerticalList<FileButton46UiElements>(mapper.Get<ScrollRect>("Scroll View"), mapper.GetChild<FileButton46UiElements>("FileButton"));

    }
}



public class FileButton46UiElements : IMappedObject, IReusableMappedObject
{
    public IMapper Mapper { get; private set; }
    public Button Button { get; private set; }
    public TMP_Text Text { get; private set; }
    public GameObject Root { get; private set; }

    public FileButton46UiElements() { }
    public FileButton46UiElements(IMapper mapper) { Initialize(mapper); }

    public void Initialize(IMapper mapper)
    {
        Mapper = mapper;
        Root = mapper.Get();
        Button = mapper.Get<Button>();
        Text = mapper.Get<TMP_Text>("FileText");
    }

    public void Activate()
    {
    }

    public void Deactivate()
    {
        Button.onClick.RemoveAllListeners();
    }
}
