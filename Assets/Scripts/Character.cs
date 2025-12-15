using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Base stat template")]
    public EntityScriptableObject entityData;  // ebből jön az alap egészség (később más is jöhet)

    [Header("Equipped weapon")]
    public WeaponScriptableObject weapon;     // külön slot

    [Header("Equipped armors")]
    public List<ArmorScriptableObject> equipment = new(); // minden armor típusú felszerelés

    [Header("Runtime stats (calculated)")]
    public int maxHealth;
    public int currentHealth;
    public int damage;          // physical damage
    public int magicDamage;     // ha kell később
    public int defense;         // később bővülhet
    public int magicDefense;
    public int range;
    public int baseMoveSpeed;       // EntityScriptableObject-ből jön
    public int finalMoveSpeed;
    public Tile currentTile;
    public Room currentRoom;// Ezzel dolgozunk harcban

    protected virtual void Awake()
    {
        RecalculateStats();
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Újraszámolja az összes statot az EntitySO + WeaponSO + Equipment alapján
    /// </summary>
    public virtual void RecalculateStats()
    {
        // ----- Base Health az EntitySciptableObject-ből -----
        maxHealth = entityData != null ? entityData.maxhealth : 100;

        // ----- Stat alapértékek (felülírhatók entityData-ból vagy később specializálva) -----
        damage = 10;
        magicDamage = 0; // még nincs mágikus sebzés forrás
        defense = 0;
        magicDefense = 0;
        range = 1;
        int totalSpeedBonus = 0;
        int totalWeightPenalty = 0;

        foreach (var armor in equipment)
        {
            if (armor == null) continue;
            maxHealth += armor.health;
            defense += armor.defense;
            magicDefense += armor.magic_defense;
        }


        if (weapon != null)
        {
            damage += weapon.damage;
            range = weapon.range > 0 ? weapon.range : range;
            // magicDamage később, ha lesz a WeaponSO-ban
        }

        // Current HP ne ugorjon max fölé
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
        finalMoveSpeed = baseMoveSpeed + totalSpeedBonus - totalWeightPenalty;
        finalMoveSpeed = Mathf.Max(1, finalMoveSpeed); 

    }

    // ------------ Equipment kezelő metódusok ----------------

    public void EquipArmor(ArmorScriptableObject armor)
    {
        if (armor == null) return;
        if (!equipment.Contains(armor))
        {
            equipment.Add(armor);
            RecalculateStats();
        }
    }
    public IEnumerator MoveInCombat(Tile targetTile)
    {
        Debug.Log("MoveInCombat called!");
        Debug.Log("currentTile = " + currentTile);
        Debug.Log("targetTile = " + targetTile);
        Debug.Log("currentRoom = " + currentRoom);
        if (currentRoom == null || targetTile == null)
            yield break;

        // Path lekérése (Character közös Pathfinding osztályt használ)
        List<Tile> fullPath = Pathfinding.GetPath(currentTile, targetTile, currentRoom);

        // Ha no path or already standing on it
        if (fullPath == null || fullPath.Count <= 1)
            yield break;

        // Első Tile mindig a jelenlegi – kihagyjuk
        fullPath.RemoveAt(0);

        // Csak a mozgástartományon belüli lépések
        int stepsAllowed = Mathf.Clamp(finalMoveSpeed, 1, fullPath.Count);
        List<Tile> limitedPath = fullPath.GetRange(0, stepsAllowed);

        // 🔹 Lépésenkénti animált mozgás
        foreach (Tile step in limitedPath)
        {
            while (Vector3.Distance(transform.position, step.transform.position) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    step.transform.position,
                    entityData.moveSpeed * Time.deltaTime
                );
                yield return null;
            }

            // Biztonságos pozíció beállítás
            transform.position = step.transform.position;

            // Az aktuális tile frissítése
            currentTile = step;

            yield return new WaitForSeconds(0.05f);
        }

        // 👉 Ha végzett a mozgással, jelezzük a BattleManager-nek
        BattleManager.Instance.SafeNextTurn(this);

    }


    public bool IsInRange(Character target)
    {
        Debug.Log("Current tile: "+currentTile);
        Debug.Log("Current Room: " + currentRoom);
        Debug.Log("Target : " + target);
        int dx = Mathf.Abs(currentTile.position.x - target.currentTile.position.x);
        int dy = Mathf.Abs(currentTile.position.y - target.currentTile.position.y);
        Debug.Log("Dx: "+ dx);
        Debug.Log("Dy: " + dy);

        return Mathf.Max(dx, dy) <= weapon.range; // Manhattan helyett sima range
    }

    public void UpdateCurrentRoom()
    {
        foreach (var room in FindObjectsOfType<Room>())
        {
            if (room.tileMap.ContainsValue(currentTile))
            {
                currentRoom = room;
                return;
            }
        }
    }


    public void UnequipArmor(ArmorScriptableObject armor)
    {
        if (armor == null) return;
        if (equipment.Remove(armor))
        {
            RecalculateStats();
        }
    }

    


    public void SetWeapon(WeaponScriptableObject newWeapon)
    {
        weapon = newWeapon;
        RecalculateStats();
    }

    // ------------ Stat frissítő harc közben ----------------

    public virtual void TakeDamage(int physicalDamage, int magicDamage)
    {
        int finalPhysical = Mathf.Max(0, physicalDamage - defense);
        int finalMagic = Mathf.Max(0, magicDamage - magicDefense);

        int totalDamage = finalPhysical + finalMagic;

        currentHealth -= totalDamage;

        if (currentHealth < 0)
            currentHealth = 0;
        DamagePopup.Create(transform.position, totalDamage);

        // 🔥 Flash Effect indítása
        StartCoroutine(HitFlash());
        if (IsDead)
        {
            OnDeath();
        }

    }
    IEnumerator HitFlash()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.red;   // 💥 találat — piros
            yield return new WaitForSeconds(0.2f);
            sr.color = Color.white; // vissza fehérre
        }
    }

    public virtual void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    public void FullHeal()
    {
        currentHealth = maxHealth;
    }

    public float HealthPercent => (float)currentHealth / maxHealth;
    public bool IsDead => currentHealth <= 0;
    public virtual void OnDeath()
    {
        // BattleManager értesítése
        BattleManager.Instance.OnCharacterDeath(this);
    }


}
