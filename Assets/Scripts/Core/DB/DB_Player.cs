using UnityEngine;

/// <summary>
/// TODO: DB_UserEにまとめるべきでは？
/// </summary>
[CreateAssetMenu(fileName = "DB_Player", menuName = "DB/DB_Player")]

public class DB_Player : ScriptableObject
{
    public Transform worldRoot;
    public Transform user;
    public (int, int, int) chunk;
}
