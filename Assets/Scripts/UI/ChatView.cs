using AnKuchen.KuchenList;
using AnKuchen.Map;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DC;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatView : MonoBehaviour
{
    const float delayTimeSec = 2.0f;
    [SerializeField] UICache root;
    [SerializeField] Transform content;
    [SerializeField] Transform playView;
    [SerializeField] ChatMessageView messagePrefab;
    [SerializeField] ChatMessageView messagePrefabLeft;
    TextChatUiElements ui;
    private DB_Chat chat;
    private DB_User player;

    void Start()
    {
        ui = new TextChatUiElements(root);
        GM.Add("ShowChat", Show);
        GM.Add("CloseChat", Close);
        // GM.Add<ChatMessageContent>("ChatAddMessage", AddMessage);
        chat = GM.db.chat;
        player = GM.db.user;
        ui.Input.onSubmit.AddListener((text) => OnInputText(text));
        GM.Add<string, string>("AddChatMessage", AddChatMessage);
    }

    void Show()
    {
        ui.Toggle.active = true;
        ui.Input.Select();

        content.DestroyChildren();
        foreach (var data in chat.data)
        {
            var user = player.GetData(data.uid);
            if (user == null) continue;

            ChatMessageContent message;
            message.content = data.content;
            message.userImage = user.thumbnail;
            message.userName = user.name;
            var leftSide = data.uid != GM.db.rtc.id;
            AddChatView(message, leftSide);
        }
    }

    void Close()
    {
        ui.Toggle.active = false;
    }

    void OnInputText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            ui.Input.ActivateInputField();
            return;
        }

        //if (text.Substring(0, 1) == "/")
        //{
        //    string systemText = GM.ExecuteUserCommand(text.Substring(1));
        //    ChatMessageContent systemMessage;
        //    systemMessage.content = systemText;
        //    systemMessage.userImage = null;
        //    systemMessage.userName = null;
        //    AddChatView(systemMessage, true);
        //    ui.Input.text = "";
        //    ui.Input.ActivateInputField();
        //    return;
        //}

        if (text == "/get")
        {
            // 全ClientにLogを要求
            GM.Msg("GetAllLog");
            AddChatMessage("", text);
            ui.Input.text = "";
            ui.Input.ActivateInputField();
            return;
        }

        var uid = GM.db.rtc.id;
        AddChatMessage(uid, text);
        GM.Msg("SendChat", uid, text);

        ui.Input.text = "";
        ui.Input.ActivateInputField();
    }

    /// <summary>
    /// ログと右上に追加する
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="text"></param>
    void AddChatMessage(string uid, string text)
    {
        var user = player.GetData(uid);

        if (user == null)
        {
            Debug.LogError($"{uid} user not found");
            return;
        }

        SaveMessage(uid, text);

        ChatMessageContent message;
        message.content = text;
        message.userImage = user.thumbnail;
        message.userName = user.name;

        // 他人であれば左に表示する
        var leftSide = uid != GM.db.rtc.id;
        AddMessage(message, leftSide);
    }

    private void SaveMessage(string uid, string text)
    {
        var data = new DB_ChatMessageE();
        data.content = text;
        data.uid = uid;
        chat.data.Add(data);
    }

    void AddMessage(ChatMessageContent message, bool leftSide = false)
    {
        AddChatView(message, leftSide);
        AddPlayView(message);
    }

    void AddChatView(ChatMessageContent message, bool leftSide = false)
    {
        var obj = Instantiate(!leftSide ? messagePrefab : messagePrefabLeft, content);
        obj.SetData(message);
    }

    void AddPlayView(ChatMessageContent message)
    {
        var obj = Instantiate(messagePrefab, playView);
        obj.SetData(message);
        obj.DestroyDelay(delayTimeSec).Forget();
    }
}

public class TextChatUiElements : IMappedObject
{
    public IMapper Mapper { get; private set; }
    public GameObject Root { get; private set; }
    public Button Button { get; private set; }
    public UI_ToggleFade Toggle { get; private set; }
    public TMP_InputField Input { get; private set; }
    // public VerticalList<ChatMessage10237RUiElements> SelectList { get; private set; }
    public TextChatUiElements() { }
    public TextChatUiElements(IMapper mapper) { Initialize(mapper); }

    public void Initialize(IMapper mapper)
    {
        Mapper = mapper;
        Root = mapper.Get();
        Button = mapper.Get<Button>("Button");
        Toggle = mapper.Get<UI_ToggleFade>();
        Input = mapper.Get<TMP_InputField>("ChatInputField");
        // SelectList = new VerticalList<ChatMessage10237RUiElements>(mapper.Get<ScrollRect>("MessageScrollView"), mapper.GetChild<ChatMessage10237RUiElements>("ChatMessage [102_37]R"));
    }
}

public class ChatMessage10237RUiElements : IMappedObject
{
    public IMapper Mapper { get; private set; }
    public GameObject Root { get; private set; }
    public TMP_Text Content { get; private set; }
    public TMP_Text UserName { get; private set; }
    public Image UserImage { get; private set; }
    public ChatMessage10237RUiElements() { }
    public ChatMessage10237RUiElements(IMapper mapper) { Initialize(mapper); }

    public void Initialize(IMapper mapper)
    {
        Mapper = mapper;
        Root = mapper.Get();
        Content = mapper.Get<TMP_Text>("Content [102:33]");
        UserName = mapper.Get<TMP_Text>("UserName [102:34]");
        UserImage = mapper.Get<Image>("UserImage [102:35]");
    }
}
