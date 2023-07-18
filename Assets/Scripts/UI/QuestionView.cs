using AnKuchen.KuchenLayout;
using AnKuchen.KuchenList;
using AnKuchen.Map;
using Cysharp.Threading.Tasks;
using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionView : MonoBehaviour
{
    [SerializeField] UICache root;
    private QuestionUiElements ui;

    void Start()
    {
        GM.Add<string>("QuestionTitle", SetTitle);
        GM.Add<string[], UniTask<int>>("Question", Show);
        ui = new QuestionUiElements(root);
        SetTitle("");
    }

    private void SetTitle(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            ui.titleText.gameObject.SetActive(false);
            return;
        }

        ui.titleText.gameObject.SetActive(true);
        ui.titleText.text = text;
    }

    async UniTask<int> Show(string[] selects)
    {
        ui.toggle.Show();
        var buttonClick = new UniTaskCompletionSource<int>();
        using (var editor = ui.SelectList.Edit())
        {
            for(var i = 0; i < selects.Length; i++)
            {
                var button = editor.Create();
                button.Text.text = selects[i];
                var i1 = i;
                button.Button.onClick.AddListener(() => buttonClick.TrySetResult(i1));

            }
        }

        var selectedIndex = await buttonClick.Task;
        
        ui.toggle.Close();
        return selectedIndex;
    }
}

public class QuestionUiElements : IMappedObject
{
    public IMapper Mapper { get; private set; }
    public GameObject Root { get; private set; }
    public TMP_Text titleText { get; private set; }
    public Button SelectButton { get; private set; }
    public UI_ToggleFade toggle { get; private set; }
    public Layout<SelectButtonUiElements> SelectList { get; private set; }

    public QuestionUiElements() { }
    public QuestionUiElements(IMapper mapper) { Initialize(mapper); }

    public void Initialize(IMapper mapper)
    {
        Mapper = mapper;
        Root = mapper.Get();
        SelectButton = mapper.Get<Button>("SelectButton");
        titleText = mapper.Get<TMP_Text>("TitleText");
        toggle = mapper.Get<UI_ToggleFade>();
        SelectList = new Layout<SelectButtonUiElements>(mapper.GetChild<SelectButtonUiElements>("SelectButton"));

    }
}

public class SelectButtonUiElements : IMappedObject
{
    public IMapper Mapper { get; private set; }
    public GameObject Root { get; private set; }
    public Button Button { get; private set; }
    public TMP_Text Text { get; private set; }
    public SelectButtonUiElements() { }
    public SelectButtonUiElements(IMapper mapper) { Initialize(mapper); }

    public void Initialize(IMapper mapper)
    {
        Mapper = mapper;
        Root = mapper.Get();
        Button = mapper.Get<Button>();
        Text = mapper.Get<TMP_Text>("SelectText");
    }
}
