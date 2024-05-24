using DC;
using Teo.AutoReference;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldSelector : UIComponent
{
    [Get, SerializeField] private UIEasingAnimationPosition animation;
    [GetInChildren, SerializeField] private TMP_InputField input;
    [GetInChildren, SerializeField] private Button submitButton;

    void Start()
    {
        submitButton.onClick.AddListener(() => OnSubmit());
        GM.Add<string>("UpdateWorldID", UpdateWorldID);
    }

    private void UpdateWorldID(string worldID)
    {
        input.text = worldID;
    }

    private void OnSubmit()
    {
        GM.Msg("GenerateWorld", input.text);
        Close();
    }

    public override void Show()
    {
        base.Show();
        animation.Show();
    }

    public override void Close()
    {
        base.Close();
        animation.Close();
    }
}
