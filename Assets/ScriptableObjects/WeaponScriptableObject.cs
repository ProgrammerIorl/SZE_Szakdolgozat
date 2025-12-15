using System.Collections;
using System.Collections.Generic;

using UnityEngine;
[CreateAssetMenu(fileName = "WeaponScriptableObject", menuName = "ScriptableObjects/Weapon")]

public class WeaponScriptableObject : EquipmentScriptableObject
{
    public enum ElementType { None,Fire, Water, Earth, Wind };
    public int damage;
    public int magic_damage;
    public ElementType element;
    public int range;

}
