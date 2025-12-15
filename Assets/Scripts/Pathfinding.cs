using System.Collections.Generic;
using UnityEngine;

public static class Pathfinding
{
    // 🟢 KOMPA kompatibilitási wrapper — Character.cs ezt hívja
    public static List<Tile> GetPath(Tile start, Tile end, Room room)
    {
        return FindPath(start, end, room); // belül már A* fut!
    }


    // 🟢 TELJES A* ALGORITMUS
    public static List<Tile> FindPath(Tile start, Tile end, Room room)
    {
        if (start == null || end == null || room == null)
            return null;

        List<Tile> openList = new List<Tile>();
        HashSet<Tile> closedList = new HashSet<Tile>();

        openList.Add(start);
        start.gCost = 0;
        start.hCost = GetHeuristic(start, end);

        while (openList.Count > 0)
        {
            Tile current = GetLowestFCostTile(openList);

            if (current == end)
                return ReconstructPath(end);

            openList.Remove(current);
            closedList.Add(current);

            foreach (Tile neighbor in GetNeighbors(current, room))
            {
                if (neighbor == null ||
                    neighbor.so.type == TileType.Wall ||
                    neighbor.so.type == TileType.Debris)
                    continue;

                if (closedList.Contains(neighbor))
                    continue;

                int movementCost = GetMovementCost(neighbor);
                int tentativeGCost = current.gCost + movementCost;

                if (tentativeGCost < neighbor.gCost || !openList.Contains(neighbor))
                {
                    neighbor.gCost = tentativeGCost;
                    neighbor.hCost = GetHeuristic(neighbor, end);
                    neighbor.parent = current;

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }

        return null;
    }

    // 🟢 MOZGÁSKÖLTSÉGEK (Water drága, Floor olcsó)
    private static int GetMovementCost(Tile tile)
    {
        return tile.so.type switch
        {
            TileType.Water => 4,
            TileType.Plant => 2,
            TileType.Plain => 1,
            TileType.Door => 1,
            _ => int.MaxValue
        };
    }

    // 🧮 Manhattan heurisztika
    private static int GetHeuristic(Tile a, Tile b)
    {
        return Mathf.Abs(a.position.x - b.position.x) +
               Mathf.Abs(a.position.y - b.position.y);
    }

    // 🔎 Szomszédos tile-ok (4 irány)
    private static List<Tile> GetNeighbors(Tile tile, Room room)
    {
        List<Tile> neighbors = new List<Tile>();
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in dirs)
        {
            Vector2Int checkPos = tile.position + dir;
            if (room.tileMap.TryGetValue(checkPos, out Tile neighbor))
                neighbors.Add(neighbor);
        }
        return neighbors;
    }

    private static Tile GetLowestFCostTile(List<Tile> openList)
    {
        Tile lowest = openList[0];
        foreach (var t in openList)
        {
            if (t.fCost < lowest.fCost || (t.fCost == lowest.fCost && t.hCost < lowest.hCost))
                lowest = t;
        }
        return lowest;
    }

    private static List<Tile> ReconstructPath(Tile end)
    {
        List<Tile> path = new List<Tile>();
        Tile current = end;

        while (current != null)
        {
            path.Add(current);
            current = current.parent;
        }
        path.Reverse();
        return path;
    }


    // 🟢 ⚠️ KOMPATIBILITÁSI VERZIÓ — Player.cs ezt használja
    public static List<Tile> GetTilesInRange(Tile start, int range, Room room)
    {
        List<Tile> result = new();
        Queue<(Tile tile, int dist)> queue = new();
        HashSet<Tile> visited = new();

        queue.Enqueue((start, 0));
        visited.Add(start);

        foreach (Tile t in room.tileMap.Values)
        {
            t.gCost = int.MaxValue;
            t.hCost = 0;
            t.parent = null;
        }

        var (current, dist) = queue.Dequeue();

            if (dist > range) continue;

            result.Add(current);

            foreach (var neighbor in GetNeighbors(current, room))
            {
                if (!visited.Contains(neighbor) &&
                    neighbor.so.type != TileType.Wall &&
                    neighbor.so.type != TileType.Debris)
                {
                    visited.Add(neighbor);
                    queue.Enqueue((neighbor, dist + 1));
                }
            }
        }
        return result;
    }
}
