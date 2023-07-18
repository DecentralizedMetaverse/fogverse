using AnKuchen.Map;
using DC;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_WorldSelector : MonoBehaviour
{    
    [SerializeField] UICache root;
    TMP_InputField input;
    private UI_ShowCloseFade ui;

    void Start()
    {
        var button = root.Get<Button>("SubmitButton");
        input = root.Get<TMP_InputField>("WorldInputField");
        button.onClick.AddListener(() => OnSubmit());

        ui = root.Get<UI_ShowCloseFade>();
        GM.Add("ShowWorldSelector", () =>
        {
            ui.active = ui.active ? false : true;
        }, true);

        GM.Add<string>("UpdateWorldID", UpdateWorldID);
    }

    void UpdateWorldID(string worldID)
    {
        input.text = worldID;
    }

    void OnSubmit()
    {
        GM.Msg("GenerateWorld", input.text);
        ui.active = false;
    }
}
