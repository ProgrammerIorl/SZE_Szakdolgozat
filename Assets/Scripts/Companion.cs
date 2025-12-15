using UnityEngine;
using System.Collections;


public class Companion : Character
{
    public enum ControlMode { Manual, ML }
    public ControlMode controlMode = ControlMode.ML;

    protected override void Awake()
    {
        base.Awake();
    }

    public void DecideAction()
    {
        if (controlMode == ControlMode.ML)
        {
            Debug.Log($"{name} AI módon cselekszik.");
            BattleManager.Instance.StartCoroutine(AutoAction());
        }
        else
        {
            Debug.Log($"{name} manuális vezérlésre váltott.");
            BattleManager.Instance.StartCompanionTurn(); // 🔥 UI aktiválása
        }
    }

    private IEnumerator AutoAction()
    {
        Character target = BattleManager.Instance.GetClosestEnemy(this);

        if (target != null && !IsInRange(target))
        {
            yield return BattleManager.Instance.StartCoroutine(MoveInCombat(target.currentTile));
        }
        else if (target != null)
        {
            target.TakeDamage(damage, magicDamage);
        }

        BattleManager.Instance.NextTurn();
    }
}
