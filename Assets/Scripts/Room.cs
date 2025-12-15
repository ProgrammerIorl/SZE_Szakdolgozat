using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public enum Direction { North, South, East, West, None }
public enum TileType { Plain, Water, Plant, Debris, Wall, Door }

public class Room : MonoBehaviour
{
    private int noiseOffsetX;
    private int noiseOffsetY;
    public Dictionary<Vector2Int, Tile> tileMap = new();
    public int x;
    public int y;
    public Vector3 CameraPlacement;
    public List<Direction> availableDirections = new List<Direction>();
    public float scale = 0.3f;
    public bool hasEnemy;
    readonly MapGenerator MG = MapGenerator.Instance;
    public Dictionary<Direction, Tile> doors = new Dictionary<Direction, Tile>();
    public Dictionary<Direction, Room> neighbors = new Dictionary<Direction, Room>();


    
    public int GetXCoord() => x;
    public int GetYCoord() => y;
    public void SetXCoord(int value) => x = value;
    public void SetYCoord(int value) => y = value;

    private void Awake()
    {
        noiseOffsetX = UnityEngine.Random.Range(0, 1000);
        noiseOffsetY = UnityEngine.Random.Range(0, 1000);
        AddAvailableDirections();

    }

    private void Start()
    {
        CameraPlacement = new Vector3(x + 5, y + 5,-1);
    }

    bool IsEdgeTile(int x, int z) => x == 0 || x == 10 || z == 0 || z == 10;
    public void RegisterTile(Tile tile)
    {
        tileMap[new Vector2Int(tile.position.x, tile.position.y)] = tile;
    }
    public void AddAvailableDirections()
    {
        availableDirections.Add(Direction.North);
        availableDirections.Add(Direction.South);
        availableDirections.Add(Direction.West);
        availableDirections.Add(Direction.East);
    }

    public static Vector2 DirectionToOffset(Direction dir, float distance) => dir switch
    {
        Direction.North => new Vector2(0, distance),
        Direction.South => new Vector2(0, -distance),
        Direction.East => new Vector2(distance, 0),
        Direction.West => new Vector2(-distance, 0),
        _ => Vector2.zero
    };

    public void GenerateRoomTiles()
    {
        for (int i = 0; i <= 10; i++)
        {
            for (int j = 0; j <= 10; j++)
            {
                Vector2Int pos = new(i, j);
                if (!tileMap.TryGetValue(pos, out Tile tile))
                {
                    continue;
                }

                if (IsEdgeTile(i, j))
                {

                    tile.so = MG.types.First(t => t.name == "Wall");
                }
                else
                    AssignPerlinType(tile, tile.position.x + i, tile.position.y + j);
                ApplyTileType(tile);
            }
        }


    }
    public void GenerateDoorsAndPaths()
    {
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            if (!availableDirections.Contains(dir))
            {
                Vector2Int pos = Vector2Int.zero;
                List<Vector2Int> pathPositions = new();

                switch (dir)
                {
                    case Direction.North:
                        pos = new Vector2Int(5, 10);
                        pathPositions = Enumerable.Range(5, 5).Select(i => new Vector2Int(5, i)).ToList();
                        break;

                    case Direction.South:
                        pos = new Vector2Int(5, 0);
                        pathPositions = Enumerable.Range(1, 5).Select(i => new Vector2Int(5, i)).ToList();
                        break;

                    case Direction.West:
                        pos = new Vector2Int(0, 5);
                        pathPositions = Enumerable.Range(1, 5).Select(i => new Vector2Int(i, 5)).ToList();
                        break;

                    case Direction.East:
                        pos = new Vector2Int(10, 5);
                        pathPositions = Enumerable.Range(5, 5).Select(i => new Vector2Int(i, 5)).ToList();
                        break;
                }

                // 🔹 Path tiles biztonságos beállítása
                foreach (var p in pathPositions)
                {
                    if (tileMap.TryGetValue(p, out Tile pathTile) && pathTile != null)
                    {
                        pathTile.so = MapGenerator.Instance.types.First(t => t.name == "Plain");
                        pathTile.tag = "Untagged";
                        ApplyTileType(pathTile);
                    }
                }

                // 🔹 Door tile biztonságos beállítása
                if (pos != Vector2Int.zero && tileMap.TryGetValue(pos, out Tile doorTile) && doorTile != null)
                {
                    doorTile.so = MapGenerator.Instance.types.First(t => t.name == "Door");
                    ApplyTileType(doorTile);

                    if (!doors.ContainsKey(dir))
                        doors.Add(dir, doorTile);
                }
            }
        }
    }

    public void ConnectDoors()
    {
        tileMap.TryGetValue(new Vector2Int(5, 5), out Tile tile);
        tile.so = MG.types.First(t => t.name == "Plain");

    }


    void AssignPerlinType(Tile tile, int localX, int localY)
    {
        float sampleX = (localX + x + noiseOffsetX) * scale;
        float sampleY = (localY + y + noiseOffsetY) * scale;
        float noiseValue = Mathf.PerlinNoise(sampleX, sampleY);

        tile.so = noiseValue switch
        {
            < 0.13f => MG.types.First(t => t.name == "Water"),
            < 0.26f => MG.types.First(t => t.name == "Plant"),
            < 0.31f => MG.types.First(t => t.name == "Debris"),
            _ => MG.types.First(t => t.name == "Plain")
        };
    }

    public void SetNeighbor(Direction dir, Room neighbor)
    {
        neighbors[dir] = neighbor;
    }


    public void ApplyTileType(Tile tile)
    {
        
        if (tile == null) return;
        var match = MG.types.FirstOrDefault(t => t.type == tile.so.type);

        if (match != null)
        {
            if (match.type == TileType.Wall || match.type == TileType.Debris)
            {
                tile.tag = "Wall";
            }
            if (match.type == TileType.Plant)
            {
                tile.tag = "Plant";
            }
            
            tile.GetComponent<SpriteRenderer>().sprite = match.sprite;
        }


        tile.gameObject.layer = tile.so.type switch
        {
            TileType.Wall or TileType.Debris => LayerMask.NameToLayer("Unwalkable"),
            _ => LayerMask.NameToLayer("Ground"),
        };

    }
}
