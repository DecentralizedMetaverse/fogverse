using AnKuchen.KuchenLayout;
using AnKuchen.Map;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DC;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AnKuchen.KuchenList;

/// <summary>
/// 所持しているContentから世界にObjectを配置するUI
/// </summary>
public class UI_ContentManager : MonoBehaviour
{
    [SerializeField] UICache root;
    ContentManagerUiElements ui;
    private string contentPath;
    private string[] files;

    void Start()
    {
        ui = new ContentManagerUiElements(root);
        contentPath = $"{Application.dataPath}/{GM.mng.contentPath}";

        GM.Add("ShowContentManager", Show, true);

        var button = root.Get<Button>("SubmitButton");
        button.onClick.AddListener(()=> OnOpenFolder());
    }

    void Show()
    {
        if (ui.ui.active) { ui.ui.active = false; return; }

        // ディレクトリ内のFile一覧を取得
        files = Directory.GetFiles(contentPath);
        using (var editor = ui.ItemList.Edit())
        {
            // File一覧を表示
            int i = 0;
            foreach (string filePath in files)
            {
                var i1 = i;
                editor.Contents.Add(new UIFactory<FileButtonUiElements>(button =>
                {
                    button.Text.text = Path.GetFileName(filePath);
                    button.Button.onClick.AddListener(() => OnSubmit(i1));

                }));
                i++;
            }
        }

        ui.ui.active = true;
    }

    void OnSubmit(int i)
    {
        // ContentをWorldに生成する
        string source = files[i];
        GM.Msg("GenerateObj", source);

        // 生成したObjectを設定Fileに保存する
        // (注意: 全Objectの設定Fileを更新する)
        GM.Msg("RegisterObject");
    }

    void OnOpenFolder()
    {
        System.Diagnostics.Process.Start(contentPath);
    }
}


public class ContentManagerUiElements : IMappedObject
{
    public IMapper Mapper { get; private set; }
    public GameObject Root { get; private set; }
    public Button SubmitButton { get; private set; }
    public Button FileButton { get; private set; }
    public UI_ToggleFade ui { get; private set; }

    public VerticalList<FileButtonUiElements> ItemList { get; private set; }
    public ContentManagerUiElements() { }
    public ContentManagerUiElements(IMapper mapper) { Initialize(mapper); }

    public void Initialize(IMapper mapper)
    {
        Mapper = mapper;
        Root = mapper.Get();
        SubmitButton = mapper.Get<Button>("SubmitButton");
        FileButton = mapper.Get<Button>("FileButton");
        ItemList = new VerticalList<FileButtonUiElements>(mapper.Get<ScrollRect>("Scroll View"),mapper.GetChild<FileButtonUiElements>("FileButton"));
        ui = mapper.Get<UI_ToggleFade>();
    }
}


public class FileButtonUiElements : IMappedObject, IReusableMappedObject
{
    public IMapper Mapper { get; private set; }
    public GameObject Root { get; private set; }
    public Button Button { get; private set; }
    public TMP_Text Text { get; private set; }

    public FileButtonUiElements() { }
    public FileButtonUiElements(IMapper mapper) { Initialize(mapper); }

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
