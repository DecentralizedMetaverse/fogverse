using System;
using AnKuchen.KuchenLayout;
using AnKuchen.Map;
using DC;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main Menu
/// </summary>
[Obsolete]
public class Menu : MonoBehaviour
{
    [SerializeField] bool show;
    [SerializeField] protected string uiName;
    [SerializeField] UICache root;
    [SerializeField] Sprite[] subMenuIcon;
    [SerializeField] protected string[] subMenu;
    MenuUiElements menuUI;

    //List<string> subMenu = new()
    //{
    //    "ShowWorldSelector",
    //    "ShowContentManager",
    //    "ShowQRCodeReader",
    //    "ShowContentImporter",
    //    "ShowSettings",
    //};

    void Start()
    {
        menuUI = new MenuUiElements(root);
        AddMsg();
        if (show) Show();
    }

    protected virtual void AddMsg()
    {
        GM.Add($"Show{uiName}", Show);
        GM.Add($"Close{uiName}", Close);
    }

    protected virtual void Show()
    {
        menuUI.ui.active = true;
        using (var editor = menuUI.ItemList.Edit())
        {
            for (int i = 0; i < subMenu.Length; i++)
            {
                var i1 = i;
                var button = editor.Create();
                button.Icon.sprite = subMenuIcon[i1];
                button.Button.onClick.AddListener(() => OnClick(i1));
            }
        }
    }

    protected virtual void Close()
    {
        menuUI.ui.active = false;
    }

    protected virtual void OnClick(int i)
    {
        GM.Msg(subMenu[i]);
    }
}

public class MenuUiElements : IMappedObject
{
    public IMapper Mapper { get; private set; }
    public GameObject Root { get; private set; }
    public Button MenuButton { get; private set; }
    public UI_ShowCloseFade ui { get; private set; }
    public Layout<MenuButtonUiElements> ItemList { get; private set; }


    public MenuUiElements() { }
    public MenuUiElements(IMapper mapper) { Initialize(mapper); }

    public void Initialize(IMapper mapper)
    {
        Mapper = mapper;
        Root = mapper.Get();
        MenuButton = mapper.Get<Button>("MenuButton");
        ItemList = new Layout<MenuButtonUiElements>(mapper.GetChild<MenuButtonUiElements>("MenuButton"));
        ui = mapper.Get<UI_ShowCloseFade>();
    }
}


public class MenuButtonUiElements : IMappedObject
{
    public IMapper Mapper { get; private set; }
    public GameObject Root { get; private set; }
    public Button Button { get; private set; }
    public Image Icon { get; private set; }

    public MenuButtonUiElements() { }
    public MenuButtonUiElements(IMapper mapper) { Initialize(mapper); }

    public void Initialize(IMapper mapper)
    {
        Mapper = mapper;
        Root = mapper.Get();
        Button = mapper.Get<Button>();
        Icon = mapper.Get<Image>("IconImage");
    }
}
