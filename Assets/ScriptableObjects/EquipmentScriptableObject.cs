using UnityEngine;

public enum ItemRarity { Common, Rare, Epic, Legendary }

public class EquipmentScriptableObject : ScriptableObject
{
    public string equipment_name;
    public string description;
    public Sprite sprite;
    public int level;
    public ItemRarity rarity;
    public int speedBonus;
    public int weightPenalty;

}
