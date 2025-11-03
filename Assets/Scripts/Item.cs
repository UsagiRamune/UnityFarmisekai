using TriInspector;
using UnityEngine;

//1-100 seed
//101-200 crop
//201-300 tool
//301-400 building
//401-500 equipment
//501–600 Weapon
//601-700 material
//701-800 misc
public enum ItemType
{
    Seed,
    Crop,
    Tool,
    Building,
    Equipment,
    Weapon,
    Material,
    Misc
}
public enum ItemAction
{
    None,
    Plant,
    Water,
    Harvest,
    Build,
    Attack
}
[CreateAssetMenu(menuName = "New Item")]
public class Item : ScriptableObject
{
    public int iD;
    public string itemname;
    public ItemType type;
    public ItemAction action;
    public Sprite sprite;
    public bool stackable = true;
    [ShowIf(nameof(stackable))]
    public int maxStack = 99;
    
}
