using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DB_Chat", menuName = "DB/DB_Chat")]
public class DB_Chat : ScriptableObject
{
    public List<DB_ChatMessageE> data = new ();
}

[System.Serializable]
public class DB_ChatMessageE
{
    public string uid;
    public string content;
}
public struct ChatMessageContent
{
    public string userName;
    public Sprite userImage;
    public string content;
}