using AnKuchen.KuchenLayout;
using AnKuchen.KuchenList;
using AnKuchen.Map;
using DC;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class AvatarView : MonoBehaviour
{
    [SerializeField] UICache root;
    private ContentManagerUiElements ui;
    private string avatarPath;

    void Start()
    {
        avatarPath = $"{Application.dataPath}/{GM.mng.avatarPath}";
        GM.Add("ShowAvatar", Show);
        ui = new ContentManagerUiElements(root);
        var button = root.Get<Button>("SubmitButton");
        button.onClick.AddListener(() => OnOpenFolder());
    }

    void Show()
    {
        if (ui.ui.active) { ui.ui.active = false; return; }
        ui.ui.active = true;

        var files = Directory.GetFiles(avatarPath, "*.vrm");
        // ディレクトリ内のFile一覧を取得
        files = Directory.GetFiles(avatarPath);
        using (var editor = ui.ItemList.Edit())
        {
            // File一覧を表示
            foreach (string filePath in files)
            {
                editor.Contents.Add(new UIFactory<FileButtonUiElements>(button =>
                {
                    button.Text.text = Path.GetFileName(filePath);
                    button.Button.onClick.AddListener(() => OnClick(filePath));

                }));
            }
        }
    }    

    void OnClick(string filePath)
    {
        var id = GM.db.rtc.id;
        var objId = GM.db.rtc.syncObjectsByID[id][0];
        var obj = GM.db.rtc.syncObjects[objId];
        obj.SetObject(filePath);
    }

    void OnOpenFolder()
    {
        System.Diagnostics.Process.Start(avatarPath);
    }
}

//public class AvatarUiElements : IMappedObject
//{
//    public IMapper Mapper { get; private set; }
//    public GameObject Root { get; private set; }
//    public Button AvatarButton { get; private set; }
//    public UI_ToggleFade toggle { get; private set; }
//    public Layout<AvatarButtonUiElements> avatarList { get; private set; }

//    public AvatarUiElements() { }
//    public AvatarUiElements(IMapper mapper) { Initialize(mapper); }

//    public void Initialize(IMapper mapper)
//    {
//        Mapper = mapper;
//        Root = mapper.Get();
//        AvatarButton = mapper.Get<Button>("AvatarButton");
//        toggle = mapper.Get<UI_ToggleFade>();
//        avatarList = new Layout<AvatarButtonUiElements>(mapper.GetChild<AvatarButtonUiElements>("AvatarButton"));
//    }
//}

//public class AvatarButtonUiElements : IMappedObject
//{
//    public IMapper Mapper { get; private set; }
//    public GameObject Root { get; private set; }
//    public Button Button { get; private set; }
//    public Image Image { get; private set; }

//    public AvatarButtonUiElements() { }
//    public AvatarButtonUiElements(IMapper mapper) { Initialize(mapper); }

//    public void Initialize(IMapper mapper)
//    {
//        Mapper = mapper;
//        Root = mapper.Get();
//        Button = mapper.Get<Button>();
//        Image = mapper.Get<Image>();
//    }
//}
