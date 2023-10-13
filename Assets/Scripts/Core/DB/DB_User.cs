using System.Collections.Generic;
using DC;
using UnityEngine;

[CreateAssetMenu(fileName = "DB_User", menuName = "DB/DB_User")]
public class DB_User : ScriptableObject
{
    public List<DB_UserE> data = new List<DB_UserE>();
    public Dictionary<string, DB_UserE> users = new();

    public DB_UserE GetData(string id)
    {
        if (!users.ContainsKey(id))
        {
            // Dataが存在しない場合
            DB_UserE user = new DB_UserE()
            {
                id = id,
            };

            data.Add(user);
            users.Add(id, user);
        }

        return users[id];
    }

    public void AddUser(string id, string objId)
    {
        var obj = GM.db.rtc.syncObjects[objId];
        users.TryAdd(id, new DB_UserE
        {
            id = id,
            objId = objId,
            cid = obj.cid,
            transform = obj.transform,
        });
    }

    public DB_UserE GetSelfData()
    {
        return GetData(GM.db.rtc.id);
    }
}

[System.Serializable]
public class DB_UserE
{
    public string id;
    public string objId;
    public string name;
    public string cid;
    public string world;
    public Sprite thumbnail;
    public Transform transform;
    public AudioSource audioSource;
}
