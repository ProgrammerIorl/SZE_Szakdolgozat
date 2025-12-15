using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : Character
{
    public float moveSpeed = 4f;

    private List<Tile> currentPath = null;
    private bool isMoving = false;
    private bool isFollowingPath = false;

    private bool doorTravelPending = false;
    private Room doorTargetRoom;
    private Tile doorTargetTile;
    private List<Tile> movableTiles = new();
    private bool isMoveSelecting = false;

    int groundLayer;

    void Start()
    {
        groundLayer = LayerMask.GetMask("Ground");
    }

    void Update()
    {

        if (InGameMenu.IsPaused)
            return;

        if (BattleManager.Instance != null &&
            BattleManager.Instance.state == BattleManager.BattleState.Turn &&
            BattleManager.Instance.isMovementSelectionActive &&
            CurrentActorIsPlayer())
        {
            HandleCombatMovementClick();
            return;
        }

        if (BattleManager.Instance == null || BattleManager.Instance.state == BattleManager.BattleState.OutOfCombat)
        {
            if (!isMoving)
                CheckMouseClick();

            if (!isFollowingPath && currentPath != null && currentPath.Count > 0)
                StartCoroutine(FollowPath());

            return;
        }
    }
    void HandleCombatInput()
    {
        // csak akkor reagáljon kattintásra, ha mozgási tile-t választunk
        if (!isMoveSelecting) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider == null) return;

            Tile clickedTile = hit.collider.GetComponent<Tile>();
            if (clickedTile != null && movableTiles.Contains(clickedTile))
            {
                ClearHighlight();
                StartCoroutine(MoveInCombat(clickedTile));
                isMoveSelecting = false;
            }
        }
    }
    private void ClearHighlight()
    {
        foreach (Tile t in movableTiles)
        {
            t.ResetHighlight();
        }
        movableTiles.Clear();
    }


    void HandleCombatMovementClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (!hit) return;

        Tile tile = hit.collider.GetComponent<Tile>();
        if (tile != null && movableTiles.Contains(tile))
        {
            foreach (var t in movableTiles)
                t.ResetHighlight();

            movableTiles.Clear();
            BattleManager.Instance.isMovementSelectionActive = false;

            StartCoroutine(MoveInCombat(tile));
        }
    }




    void CheckMouseClick()
    {
        if (!InputManager.Instance.GetPlayerMouseInput()) return;

        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 50, groundLayer);

        if (!hit) return;

        Tile clickedTile = hit.collider.GetComponent<Tile>();
        if (clickedTile == null) return;

        if (clickedTile.so.type == TileType.Door)
            OnDoorClicked(clickedTile);
        else
            GeneratePathTo(clickedTile);
    }

    void OnDoorClicked(Tile doorTile)
    {
        Direction dir = GetDoorDirection(doorTile.position);
        if (dir == Direction.None) return;

        if (!currentRoom.neighbors.TryGetValue(dir, out Room targetRoom))
            return;

        Direction opposite = MapGenerator.Opposite(dir);
        Tile targetDoor = FindDoorInRoom(targetRoom, opposite);

        if (targetDoor == null) return;

        // Elmentjük az átlépést
        doorTravelPending = true;
        doorTargetRoom = targetRoom;
        doorTargetTile = targetDoor;

        // Útvonal a saját ajtóhoz
        GeneratePathTo(doorTile);
    }

    void GeneratePathTo(Tile target)
    {
        currentPath = FindPath(currentTile, target);
    }

    private List<Tile> FindPath(Tile start, Tile end)
    {
        List<Tile> path = new List<Tile>();
        List<Tile> openList = new List<Tile> { start };
        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();
        Dictionary<Tile, int> gScore = new Dictionary<Tile, int>();
        gScore[start] = 0;

        while (openList.Count > 0)
        {
            Tile current = openList[0];
            foreach (Tile t in openList)
            {
                if (!gScore.ContainsKey(t)) gScore[t] = int.MaxValue;
                if (gScore[t] < gScore[current]) current = t;
            }

            if (current == end) break;

            openList.Remove(current);

            foreach (Tile neighbor in GetNeighbors(current))
            {
                if (neighbor == null) continue;
                if (neighbor.so.type == TileType.Wall || neighbor.so.type == TileType.Debris) continue;

                int tentativeG = gScore[current] + 1;
                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    gScore[neighbor] = tentativeG;
                    cameFrom[neighbor] = current;
                    if (!openList.Contains(neighbor)) openList.Add(neighbor);
                }
            }
        }

        // visszafejtés
        Tile curr = end;
        while (curr != start)
        {
            path.Insert(0, curr);
            if (!cameFrom.ContainsKey(curr)) break;
            curr = cameFrom[curr];
        }
        path.Insert(0, start);

        return path;
    }
    private List<Tile> reachableTiles = new();

    public void StartCombatMovementSelection()
    {
        reachableTiles = Pathfinding.GetTilesInRange(currentTile, finalMoveSpeed, currentRoom);

        foreach (var tile in reachableTiles)
            tile.SetMoveHighlight(true);
    }
    private bool CurrentActorIsPlayer()
    {
        return BattleManager.Instance != null &&
               BattleManager.Instance.CurrentActor == this;
    }


    public void ShowMovableTiles()
    {
        if (currentRoom == null) return;

        movableTiles.Clear();
        isMoveSelecting = true;

        foreach (var tile in currentRoom.tileMap.Values)
        {
            if (tile.IsWithinRange(currentTile.position, finalMoveSpeed) &&
                tile.so.type != TileType.Wall &&
                tile.so.type != TileType.Debris)
            {
                movableTiles.Add(tile);
                tile.SetMoveHighlight(true);
            }
        }
    }


    IEnumerator FollowPath()
    {
        isMoving = true;
        isFollowingPath = true;

        while (currentPath != null && currentPath.Count > 0)
        {
            Tile nextTile = currentPath[0];

            while (Vector3.Distance(transform.position, nextTile.transform.position) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    nextTile.transform.position,
                    moveSpeed * Time.deltaTime
                );
                yield return null;
            }

            transform.position = nextTile.transform.position;
            currentTile = nextTile;

            if (currentPath.Count > 0)
                currentPath.RemoveAt(0);
            else
                break;

            yield return null;
        }

        isMoving = false;
        isFollowingPath = false;
        currentPath = null;

        if (doorTravelPending)
        {
            doorTravelPending = false;
            StartCoroutine(MoveThroughDoor());
        }
    }


    IEnumerator MoveThroughDoor()
    {
        currentRoom = doorTargetRoom;
        currentTile = doorTargetTile;
        transform.position = doorTargetTile.transform.position;
        Camera.main.transform.position = currentRoom.CameraPlacement;
        UpdateCurrentRoom();
        foreach (var companion in FindObjectsOfType<Companion>())
        {
            if (!companion.IsDead)
            {
                companion.currentRoom = doorTargetRoom;
                companion.currentTile = doorTargetTile;
                companion.transform.position = doorTargetTile.transform.position;
            }
        }

        CheckForBattle();
        yield break;
    }

    void CheckForBattle()
    {
        // ha még nincs harc
        if (BattleManager.Instance.state != BattleManager.BattleState.OutOfCombat)
            return;

        // megnézzük, van-e enemy aktuális szobában
        List<Enemy> enemiesInRoom = new List<Enemy>();

        foreach (Enemy e in FindObjectsOfType<Enemy>())
        {
            if (e.currentRoom == currentRoom && !e.IsDead)
            {
                enemiesInRoom.Add(e);
            }
        }

        if (enemiesInRoom.Count > 0)
        {
            BattleManager.Instance.BeginBattle(
                this,             // player
                FindObjectsOfType<Companion>().Where(c => c.currentRoom == currentRoom).ToList(),
                enemiesInRoom     // enemy lista
            );
        }
    }


    Direction GetDoorDirection(Vector2Int pos)
    {
        if (pos.y == 10) return Direction.North;
        if (pos.y == 0) return Direction.South;
        if (pos.x == 10) return Direction.East;
        if (pos.x == 0) return Direction.West;
        return Direction.None;
    }

    Tile FindDoorInRoom(Room room, Direction dir)
    {
        return dir switch
        {
            Direction.North => room.tileMap[new Vector2Int(5, 10)],
            Direction.South => room.tileMap[new Vector2Int(5, 0)],
            Direction.East => room.tileMap[new Vector2Int(10, 5)],
            Direction.West => room.tileMap[new Vector2Int(0, 5)],
            _ => null
        };
    }

    // Példa: szomszédos csempék lekérése (A* pathfindinghez)
    List<Tile> GetNeighbors(Tile tile)
    {
        List<Tile> neighbors = new List<Tile>();
        Vector2Int[] offsets = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var off in offsets)
        {
            Vector2Int pos = tile.position + off;
            if (currentRoom.tileMap.TryGetValue(pos, out Tile neighbor))
                neighbors.Add(neighbor);
        }
        return neighbors;
    }
}
