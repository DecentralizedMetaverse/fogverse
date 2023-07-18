using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "DB_Equip", menuName = "DB/DB_Equip")]
public class DB_Equip : ScriptableObject
{
    public List<DB_EquipE> data = new List<DB_EquipE>();
}
[System.Serializable]
public class DB_EquipE
{
    public eEquip.type key;
    public int ct_id;
    public int id;
}
