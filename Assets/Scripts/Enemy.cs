using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : Character
{
    

    public void AssignRandomEquipment(List<ArmorScriptableObject> armorPool, List<WeaponScriptableObject> weaponPool)
    {
        int diff = DifficultyManager.Instance.difficultyLevel;

        if (Random.value < DifficultyManager.Instance.GetWeaponChance())
        {
            weapon = GetRandomItemByRarity(weaponPool);
        }

        if (Random.value < DifficultyManager.Instance.GetArmorChance())
        {
            var (minArmor, maxArmor) = DifficultyManager.Instance.GetArmorCountRange();
            int armorCount = Random.Range(minArmor, maxArmor + 1);

            for (int i = 0; i < armorCount; i++)
            {
                equipment.Add(GetRandomItemByRarity(armorPool));
            }
        }

        Debug.Log($"{name} kapott: Weapon={weapon?.name ?? "None"}, Armors={equipment.Count}");
        RecalculateStats();
    }

    
    private T GetRandomItemByRarity<T>(List<T> pool) where T : EquipmentScriptableObject
    {
        float roll = Random.value;
        float c = DifficultyManager.Instance.CommonChance;
        float r = DifficultyManager.Instance.RareChance;
        float e = DifficultyManager.Instance.EpicChance;
        float l = DifficultyManager.Instance.LegendaryChance;

        ItemRarity targetRarity = ItemRarity.Common;
        /*if (roll > 1f - l) targetRarity = ItemRarity.Legendary;
        else if (roll > 1f - (l + e)) targetRarity = ItemRarity.Epic;
        else if (roll > 1f - (l + e + r)) targetRarity = ItemRarity.Rare;*/

        // Szűrés rarity alapján — LINQ működik List-nél
        List<T> candidates = pool.Where(item => item.rarity == targetRarity).ToList();

        if (candidates.Count == 0)
            candidates = pool; // fallback ha nincs ilyen rarity

        return candidates[Random.Range(0, candidates.Count)];
    }
}
