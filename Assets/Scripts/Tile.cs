using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour
{
    public TileScriptableObject so;
    public Vector2Int position;
    public Room room;
    // A* pathfinding támogatáshoz szükséges mezők
    public int gCost;    // Starttól eddig megtett távolság
    public int hCost;    // Heurisztika (becslés a célig)
    public int fCost => gCost + hCost; // A* döntési érték
    public Tile parent;  // Útvonal visszakövetéshez

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isHighlighted = false;

    // 🔹 ÚJ: highlight típus harcban
    public enum HighlightType
    {
        None,
        Move,
        Attack
    }

    public HighlightType highlightState = HighlightType.None;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        position = Vector2Int.RoundToInt(new Vector2(transform.localPosition.x, transform.localPosition.y));
        room = GetComponentInParent<Room>();
        room?.RegisterTile(this);

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void Plainify()
    {
        so = MapGenerator.Instance.types.First(t => t.name == "Plain");
        tag = "Plain";
        room.ApplyTileType(this);
    }

    // 🔹 EREDETI Highlight – MEGHAGYVA
    public void Highlight(Color color)
    {
        if (spriteRenderer == null) return;
        spriteRenderer.color = color;
        isHighlighted = true;
    }

    // 🔹 EREDETI ResetHighlight – MEGHAGYVA, csak kiegészítve
    public void ResetHighlight()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.color = originalColor;
        isHighlighted = false;
        highlightState = HighlightType.None;
    }

    // 🔹 ÚJ: mozgás highlight (move range-hez)
    public void SetMoveHighlight(bool on)
    {
        if (on)
        {
            highlightState = HighlightType.Move;
            Highlight(new Color(0.3f, 0.7f, 1f, 0.7f)); // világoskék
        }
        else
        {
            ResetHighlight();
        }
    }

    // 🔹 ÚJ: támadás highlight (attack range-hez – későbbre)
    public void SetAttackHighlight(bool on)
    {
        if (on)
        {
            highlightState = HighlightType.Attack;
            Highlight(new Color(1f, 0.3f, 0.3f, 0.7f)); // piros
        }
        else
        {
            ResetHighlight();
        }
    }

    // 🔹 ÚJ: harci input ellenőrzéshez
    public bool IsClickableMoveTile => isHighlighted && highlightState == HighlightType.Move;
    public bool IsClickableAttackTile => isHighlighted && highlightState == HighlightType.Attack;

    public bool IsWithinRange(Vector2Int origin, int range)
    {
        int distance = Mathf.Abs(position.x - origin.x) + Mathf.Abs(position.y - origin.y);
        return distance <= range;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.9f);
    }
}
