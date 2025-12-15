using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BattleManager : MonoBehaviour
{
    public enum BattleState
    {
        OutOfCombat,   
        Start,
        Turn,
        Victory,
        Defeat
    }
    public static BattleManager Instance { get; private set; }


    [Header("State")]
    public BattleState state;

    [Header("Units")]
    public Player playerUnit;
    public List<Companion> companions = new();
    public List<Enemy> enemies = new();

    [Header("UI Elements")]
    public TextMeshProUGUI infoText;
    public Button attackButton;
    public Button healButton;
    public Button skipButton;

    public Slider playerHealthBar;
    public Slider enemyHealthBar;
    public bool isMovementSelectionActive = false;

    private List<Character> turnOrder = new();
    private int turnIndex = 0;

    public Character CurrentActor => turnOrder[turnIndex];
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        state = BattleState.OutOfCombat;
        
    }


    IEnumerator SetupBattle()
    {
        infoText.text = "Harc!";
        yield return new WaitForSeconds(1f);


        turnOrder.Clear();


        if (playerUnit != null)
            turnOrder.Add(playerUnit);

        foreach (var c in companions)
        {
            if (c != null)
                turnOrder.Add(c);
        }

        foreach (var e in enemies)
        {
            if (e != null)
                turnOrder.Add(e);
        }

        turnOrder = turnOrder
        .Where(c => c != null && !c.IsDead)
        .OrderByDescending(c => c.finalMoveSpeed)
        .ToList();
        //InitHealthBars();
        state = BattleState.Turn;
        StartTurn();
    }
    public void BeginBattle(Player player, List<Companion> allies, List<Enemy> roomEnemies)
    {
        if (state != BattleState.OutOfCombat) return; // már fut harc

        playerUnit = player;
        companions = allies;
        enemies = roomEnemies;

        state = BattleState.Start;
        StartCoroutine(SetupBattle());
    }


    /*void InitHealthBars()
    {
        // Player HP bar
        playerHealthBar.maxValue = playerUnit.maxHealth;
        playerHealthBar.value = playerUnit.currentHealth;

        // Enemy HP bar: az első élő enemy-t mutatjuk
        Enemy mainEnemy = GetFirstAliveEnemy();
        if (mainEnemy != null)
        {
            enemyHealthBar.gameObject.SetActive(true);
            enemyHealthBar.maxValue = mainEnemy.maxHealth;
            enemyHealthBar.value = mainEnemy.currentHealth;
        }
        else
        {
            enemyHealthBar.gameObject.SetActive(false);
        }
    }*/

    // ---------------------------------------
    // TURN LOGIKA
    // ---------------------------------------
    void StartTurn()
    {
        if (state != BattleState.Turn) return;

        // Először ellenőrizzük, vége van-e már a csatának
        if (CheckVictoryCondition())
        {
            state = BattleState.Victory;
            EndBattle();
            return;
        }
        if (CheckDefeatCondition())
        {
            state = BattleState.Defeat;
            EndBattle();
            return;
        }

        // Ha az aktuális actor halott, lépjünk tovább
        if (CurrentActor.IsDead)
        {
            NextTurn();
            return;
        }

        // Eldöntjük, ki következik
        if (CurrentActor is Player)
        {
            StartPlayerTurn();
            Debug.Log("StartPlayerTurn");
        }
        else if (CurrentActor is Companion companion)
        {
            companion.DecideAction();   // 🔥 Automatikus döntés ML vagy manual alapján!
        }

        else if (CurrentActor is Enemy enemy)
        {
            StartCoroutine(EnemyTurn(enemy));
        }
    }
    public void StartCompanionTurn()
    {
        infoText.text = $"{CurrentActor.name} köre! Válassz akciót!";
        SetPlayerActionButtons(true); // 🔥 UI bekapcsolása (Playerhez hasonlóan)
    }



    public void NextTurn()
    {
        // Lépjünk a következő aktorra
        turnIndex = (turnIndex + 1) % turnOrder.Count;

        // Ha mindenki meghalt egy csapatból, a StartTurn majd lekezeli
        StartTurn();
    }

    // ---------------------------------------
    // PLAYER TURN
    // ---------------------------------------
    void StartPlayerTurn()
    {
        infoText.text = "A te köröd! Válassz akciót!";
        SetPlayerActionButtons(true);
    }



    public void OnAttackButton()
    {
        if (state != BattleState.Turn) return;
        if (!(CurrentActor is Player)) return; // csak akkor, ha tényleg a player köre

        SetPlayerActionButtons(false);


        StartCoroutine(PlayerAttack());
    }

    IEnumerator PlayerAttack()
    {
        infoText.text = "Támadás!";
        yield return new WaitForSeconds(0.5f);

        Enemy target = GetFirstAliveEnemy();
        if (target != null)
        {
            target.TakeDamage(playerUnit.damage, playerUnit.magicDamage);
            UpdateEnemyHealthBar();
        }

        yield return new WaitForSeconds(0.5f);

        NextTurn();
    }

    public void OnHealButton()
    {
        if (state != BattleState.Turn) return;
        if (!(CurrentActor is Player)) return;

        SetPlayerActionButtons(false);


        StartCoroutine(PlayerHeal());
    }
    public void OnSkipButton()
    {
        if (state != BattleState.Turn) return;
        if (!(CurrentActor is Player)) return;

        infoText.text = "Kör átugorva!";
        SetPlayerActionButtons(false);
        Debug.Log(infoText.text);
        NextTurn();
    }

    void SetPlayerActionButtons(bool value)
    {
        attackButton.interactable = value;
        healButton.interactable = value;
        moveButton.interactable = value;
        skipButton.interactable = value;
    }


    IEnumerator PlayerHeal()
    {
        infoText.text = "Gyógyítás!";
        yield return new WaitForSeconds(0.5f);

        playerUnit.Heal(15);
        UpdatePlayerHealthBar();

        yield return new WaitForSeconds(0.5f);

        NextTurn();
    }

    IEnumerator EnemyTurn(Enemy enemy)
    {
        infoText.text = enemy.name + " mozog...";
        yield return new WaitForSeconds(0.5f);

        Character target = GetClosestPlayerOrCompanion(enemy);

        if (target != null)
        {
            if (!enemy.IsInRange(target))
            {
                yield return StartCoroutine(enemy.MoveInCombat(target.currentTile));
                // ❗ MoveInCombat már maga hívja a FinishTurnAfterMovement-et → ami NextTurn-et hív
                yield break;  // 🔥 Itt megállítjuk, hogy NE fusson tovább
            }
            else
            {
                // Támadás esetén viszont MI hívjuk a NextTurn()-t
                target.TakeDamage(enemy.damage, enemy.magicDamage);
                UpdatePlayerHealthBar();
                Debug.Log("Enemy attacked");
            }
        }

        NextTurn(); // ⚔️ Csak támadás után van itt!
    }





    IEnumerator CompanionTurn(Companion companion)
    {
        var target = GetFirstAliveEnemy();

        if (!companion.IsInRange(target))
        {
            yield return StartCoroutine(companion.MoveInCombat(target.currentTile));
        }
        else
        {
            target.TakeDamage(companion.damage, companion.magicDamage);
        }

        yield return new WaitForSeconds(0.5f);
        NextTurn();
    }
    public Character GetClosestPlayerOrCompanion(Enemy enemy)
    {
        List<Character> possibleTargets = new List<Character>();

        if (playerUnit != null && !playerUnit.IsDead)
            possibleTargets.Add(playerUnit);

        foreach (var c in companions)
        {
            if (c != null && !c.IsDead)
                possibleTargets.Add(c);
        }

        // Ha nincs élő célpont
        if (possibleTargets.Count == 0)
            return null;

        // Megkeressük a legközelebbit Tile távolság alapján
        Character closest = possibleTargets[0];
        int closestDistance = GetTileDistance(enemy.currentTile, closest.currentTile);

        foreach (var t in possibleTargets)
        {
            int dist = GetTileDistance(enemy.currentTile, t.currentTile);
            if (dist < closestDistance)
            {
                closest = t;
                closestDistance = dist;
            }
        }

        return closest;
    }
    public Character GetClosestEnemy(Character actor)
    {
        List<Character> possibleTargets = new();

        foreach (var enemy in enemies)
        {
            if (enemy != null && !enemy.IsDead)
                possibleTargets.Add(enemy);
        }

        if (possibleTargets.Count == 0) return null;

        Character closest = possibleTargets[0];
        int closestDist = GetTileDistance(actor.currentTile, closest.currentTile);

        foreach (var target in possibleTargets)
        {
            int dist = GetTileDistance(actor.currentTile, target.currentTile);
            if (dist < closestDist)
            {
                closest = target;
                closestDist = dist;
            }
        }

        return closest;
    }


    private int GetTileDistance(Tile a, Tile b)
    {
        return Mathf.Abs(a.position.x - b.position.x) + Mathf.Abs(a.position.y - b.position.y);
    }


    // ---------------------------------------
    // VICTORY / DEFEAT CHECK
    // ---------------------------------------
    bool CheckVictoryCondition()
    {
        // Győzelem: minden enemy halott
        foreach (var e in enemies)
        {
            if (!e.IsDead) return false;
        }
        return true;
    }
    public void OnCharacterDeath(Character character)
    {
        // Ha Enemy halt meg → eltávolítjuk
        if (character is Enemy enemy)
        {
            enemies.Remove(enemy);
            Debug.Log(enemy.name + " removed from enemies list.");

            // Ha benne van a turnOrder-ben → remove
            if (turnOrder.Contains(character))
                turnOrder.Remove(character);

            Destroy(enemy.gameObject);
        }

        // Ha Companion halt meg
        else if (character is Companion companion)
        {
            companions.Remove(companion);

            if (turnOrder.Contains(character))
                turnOrder.Remove(character);

            Destroy(companion.gameObject);
        }

        // Ha Player halt meg
        else if (character is Player)
        {
            Debug.Log("Player died!");
            // majd ide jön defeat kezelése
        }

        // Ha a currentActor meghalt → lépjünk tovább
        if (CurrentActor == character)
        {
            NextTurn();
        }
    }


    bool CheckDefeatCondition()
    {
        // Vereség: a player halott ÉS minden companion halott
        if (!playerUnit.IsDead) return false;

        foreach (var c in companions)
        {
            if (!c.IsDead) return false;
        }
        return true;
    }

    void EndBattle()
    {
        SetPlayerActionButtons(false);


        if (state == BattleState.Victory) 
        infoText.text = "Győzelem!";
        else if (state == BattleState.Defeat) 
            infoText.text = "Vereség...";
    }

    // ---------------------------------------
    // HELPEREK
    // ---------------------------------------
    Enemy GetFirstAliveEnemy()
    {
        foreach (var e in enemies)
        {
            if (e != null && !e.IsDead)
                return e;
        }
        return null;
    }
    public Button moveButton;

    public void OnMoveButton()
    {
        if (state != BattleState.Turn) return;
        if (!(CurrentActor is Player)) return;

        Player player = (Player)CurrentActor;
        player.ShowMovableTiles();
        infoText.text = "Válassz egy csempét a mozgáshoz!";

        isMovementSelectionActive = true;   // 🔥 EZ KELL

        SetPlayerActionButtons(false);
    }


    public void FinishTurnAfterMovement()
    {
        if (CurrentActor is Character actor && actor == CurrentActor) // <-- fontos
            NextTurn();
    }
    public void SafeNextTurn(Character requester)
    {
        if (requester != CurrentActor) return; // ❌ más karakter coroutine-je nem léptethet!

        NextTurn(); // ✔️ csak az aktuális léptethet
    }


    void UpdatePlayerHealthBar()
    {
        //playerHealthBar.maxValue = playerUnit.maxHealth;
        //playerHealthBar.value = playerUnit.currentHealth;
    }

    void UpdateEnemyHealthBar()
    {
        Enemy mainEnemy = GetFirstAliveEnemy();
        if (mainEnemy != null)
        {
           // enemyHealthBar.gameObject.SetActive(true);
            //enemyHealthBar.maxValue = mainEnemy.maxHealth;
            //enemyHealthBar.value = mainEnemy.currentHealth;
        }
        else
        {
            //enemyHealthBar.gameObject.SetActive(false);
        }
    }
}
