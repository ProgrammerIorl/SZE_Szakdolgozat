using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [Header("Difficulty (1 = Easy, 10 = Brutal)")]
    [Range(1, 10)]
    public int difficultyLevel =10;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // --- Drop chances (scale with difficulty) ---
    public float GetWeaponChance() => Mathf.Clamp01(difficultyLevel * 0.1f);
    public float GetArmorChance() => Mathf.Clamp01(0.2f + difficultyLevel * 0.08f);

    public (int minArmor, int maxArmor) GetArmorCountRange()
    {
        if (difficultyLevel <= 3) return (0, 1);
        if (difficultyLevel <= 6) return (1, 2);
        return (2, 3);
    }

    // --- Rarity drop chances (sum must be <= 100%) ---
    public float CommonChance => Mathf.Clamp01(1f - difficultyLevel * 0.1f);
    public float RareChance => Mathf.Clamp01(difficultyLevel * 0.05f);
    public float EpicChance => Mathf.Clamp01(difficultyLevel * 0.03f);
    public float LegendaryChance => Mathf.Clamp01(difficultyLevel * 0.02f);
}
