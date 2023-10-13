using UnityEngine;

[CreateAssetMenu(fileName = "DB", menuName = "DB/DB")]
public class DB : ScriptableObject
{
    public DB_RTC rtc;
    public DB_User user;
    public DB_Player player;
    public DB_Chunk chunk;
    public DB_Chat chat;
    public DB_FunctionList functionList;

    public void Init()
    {
        rtc.Init();
    }

    public void Start()
    {
        rtc.Start();
    }

    public void End()
    {
        rtc.End();
    }
}
