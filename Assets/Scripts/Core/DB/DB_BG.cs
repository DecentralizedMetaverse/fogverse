using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DB_BG", menuName = "DB/DB_BG")]
public class DB_BG : ScriptableObject
{
    public List<DB_BGE> data = new List<DB_BGE>();
    public void Load(byte i)
    {
        if (data[i].load) return;
        if (data[i].fileName == "") return;
        data[i].sprite = Resources.Load("BG/" + data[i].fileName, typeof(Sprite)) as Sprite;
        data[i].load = true;
    }
    public void UnLoad(byte i)
    {
        if (!data[i].load) return;
        Resources.UnloadAsset(data[i].sprite);
        data[i].load = false;
    }
    public void UnLoadAll()
    {
        for (byte i = 0; i < data.Count; i++)
        {
            UnLoad(i);
        }
    }
}
[System.Serializable]
public class DB_BGE
{
    public string fileName;
    public Sprite sprite;
    public Color32 color;
    public bool load;

}
