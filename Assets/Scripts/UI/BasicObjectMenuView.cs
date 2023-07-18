using AnKuchen.KuchenLayout;
using AnKuchen.Map;
using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BasicObjectMenuView : MonoBehaviour
{
    [SerializeField] UICache root;
    [SerializeField] OjectData[] prefabs;
    BasicObjectMenuUiElements ui;
    Dictionary<string, GameObject> prefabsDict = new();

    [Serializable]
    class OjectData
    {
        public string key;
        public GameObject prefab;
    }

    void Start()
    {
        prefabsDict.Clear();
        foreach (var prefab in prefabs)
        {
            prefabsDict.Add(prefab.key, prefab.prefab);
        }

        ui = new BasicObjectMenuUiElements(root);

        GM.Add("ShowBasicObjectMenu", Show);
        GM.Add<string, bool>("IsBasicObject", (path) =>
        {
            if(prefabsDict.ContainsKey(path)) { return true; }
            return false;
        });

        GM.Add<string, Transform>("GenerateBasicObject", GenerateObject);
    }

    void Show()
    {
        if(ui.toggle.active) { ui.toggle.active = false; return; }

        using(var editor = ui.SelectList.Edit())
        {
            foreach(var menu in prefabsDict.Keys)
            {
                var button = editor.Create();
                button.Text.text = menu;
                var key = menu;
                button.Button.onClick.RemoveAllListeners(); // TODO: 本来はこの行は不要のはず　確認が必要かも
                button.Button.onClick.AddListener(() =>
                {
                    var transform = GenerateObject(key);
                    transform.SetParent(GM.db.player.worldRoot);
                    GM.Msg("RegisterObject");
                });
            }
        }

        ui.toggle.active = true;
    }

    Transform GenerateObject(string key)
    {
        var player = GM.db.player;
        var pos = player.user.position + player.user.forward;
        var obj = Instantiate(prefabsDict[key],pos, player.user.rotation);
        var objInfo = obj.AddComponent<ObjectUnknown>();
        objInfo.fileName = key;        

        return obj.transform;
    }
}

public class BasicObjectMenuUiElements : IMappedObject
{
    public IMapper Mapper { get; private set; }
    public GameObject Root { get; private set; }
    public UI_ToggleFade toggle { get; private set; }
    public Layout<BasicObjectButtonUiElements> SelectList { get; private set; }

    public BasicObjectMenuUiElements() { }
    public BasicObjectMenuUiElements(IMapper mapper) { Initialize(mapper); }

    public void Initialize(IMapper mapper)
    {
        Mapper = mapper;
        Root = mapper.Get();
        toggle = mapper.Get<UI_ToggleFade>();
        SelectList = new Layout<BasicObjectButtonUiElements>(mapper.GetChild<BasicObjectButtonUiElements>("BasicObjectButton"));
    }
}

public class BasicObjectButtonUiElements : IMappedObject
{
    public IMapper Mapper { get; private set; }
    public GameObject Root { get; private set; }
    public Button Button { get; private set; }
    public TMP_Text Text { get; private set; }

    public BasicObjectButtonUiElements() { }
    public BasicObjectButtonUiElements(IMapper mapper) { Initialize(mapper); }

    public void Initialize(IMapper mapper)
    {
        Mapper = mapper;
        Root = mapper.Get();
        Button = mapper.Get<Button>();
        Text = mapper.Get<TMP_Text>("Text");
    }
}