using DC;
using System;
using System.Collections.Generic;
using Teo.AutoReference;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BasicObjectMenuView : UIComponent
{
    [Get, SerializeField] private UIEasingAnimationPosition animation;
    [GetInChildren, Name("Content"), SerializeField] private Transform content;
    [GetInChildren, Name("BasicObjectButton"), SerializeField] private Button buttonPrefab;
    [SerializeField] OjectData[] prefabs;
    Dictionary<string, GameObject> prefabsDict = new();

    [Serializable]
    class OjectData
    {
        public string key;
        public GameObject prefab;
    }

    void Start()
    {
        buttonPrefab.gameObject.SetActive(false);

        prefabsDict.Clear();
        foreach (var prefab in prefabs)
        {
            prefabsDict.Add(prefab.key, prefab.prefab);
        }


        GM.Add("ShowBasicObjectMenu", Show);
        GM.Add<string, bool>("IsBasicObject", (path) =>
        {
            if(prefabsDict.ContainsKey(path)) { return true; }
            return false;
        });

        GM.Add<string, Transform>("GenerateBasicObject", GenerateObject);
    }

    public override void Show()
    {
        base.Show();
        animation.Show();

        content.DestroyChildren();
        foreach (var key in prefabsDict.Keys)
        {
            var button = Instantiate(buttonPrefab, content);
            button.gameObject.SetActive(true);
            button.GetComponentInChildren<TMP_Text>().text = key;
            button.onClick.AddListener(() =>
            {
                var transform = GenerateObject(key);
                transform.SetParent(GM.db.player.worldRoot);
                GM.Msg("RegisterObject");
            });
        }
    }

    public override void Close()
    {
        base.Close();
        animation.Close();
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
