using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance { get; private set; }
    public List<Room> rooms = new();
    public GameObject roomPrefab;
    public int count = 10;
    public float distance = 11f;
    int index = 0;
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject companionPrefab;

    public List<TileScriptableObject> types = new();
    public List<WeaponScriptableObject> weaponPool = new();
    public List<ArmorScriptableObject> armorPool = new();
    List<Room> availableRoomList = new();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void Start()
    {
        weaponPool.AddRange(Resources.LoadAll<WeaponScriptableObject>("Objects/WeaponTypes"));
        armorPool.AddRange(Resources.LoadAll<ArmorScriptableObject>("Objects/ArmorTypes"));
        types.AddRange(Resources.LoadAll<TileScriptableObject>("Objects/TileTypes"));
        RoomGeneration();
        SpawnUnits();
    }


    void SpawnUnits()
    {
        // --- PLAYER SPAWN ---
        Room startRoom = rooms[0];

        Tile playerTile = GetRandomWalkableTile(startRoom);
        GameObject playerObj = Instantiate(playerPrefab, playerTile.transform.position, Quaternion.identity);
        Player player = playerObj.GetComponent<Player>();

        player.currentTile = playerTile;
        player.currentRoom = startRoom;

        // --- COMPANION SPAWN (playerhez ugyanabba a szobába) ---
        if (companionPrefab != null)
        {
            Tile companionTile = GetRandomWalkableTile(startRoom);

            GameObject companionObj = Instantiate(companionPrefab, companionTile.transform.position, Quaternion.identity);
            Companion companion = companionObj.GetComponent<Companion>();

            companion.currentTile = companionTile;
            companion.currentRoom = startRoom;

            // Ha szeretnél felszerelést neki is:
           
        }

        // --- ENEMY SPAWN EACH ROOM ---
        for (int i = 1; i < rooms.Count; i++)
        {
            Room room = rooms[i];
            Tile enemyTile = GetRandomWalkableTile(room);

            GameObject enemyObj = Instantiate(enemyPrefab, enemyTile.transform.position, Quaternion.identity);
            Enemy enemy = enemyObj.GetComponent<Enemy>();

            enemy.currentTile = enemyTile;
            enemy.currentRoom = room;

            enemy.AssignRandomEquipment(armorPool, weaponPool);

        }
    }

    Tile GetRandomWalkableTile(Room room)
    {
        List<Tile> walkableTiles = new List<Tile>();

        foreach (var tile in room.tileMap.Values)
        {
            // Check walkable by tag or type
            if (tile.tag == "Wall" || tile.tag == "Debris")
                continue;

            // Optionally: check layer
            if (tile.gameObject.layer == LayerMask.NameToLayer("Unwalkable"))
                continue;

            walkableTiles.Add(tile);
        }

        if (walkableTiles.Count == 0)
        {
            Debug.LogError("Nincsen walkable tile ebben a room-ban!");
            return null;
        }

        return walkableTiles[UnityEngine.Random.Range(0, walkableTiles.Count)];
    }


    public void RoomGeneration()
    {
        Vector2 startPos = Vector2.zero;
        for (int i = 0; i < count; i++)
        {
            Room baseRoom;
            if (i == 0)
            {
                GameObject firstRoomObj = Instantiate(roomPrefab, Vector2.zero, Quaternion.identity);
                Room firstRoom = firstRoomObj.GetComponent<Room>();
                SaveRoom(firstRoom, Vector2.zero);
                availableRoomList.Add(firstRoom);
                firstRoom.GenerateRoomTiles();
                baseRoom = firstRoom;
                firstRoomObj.name = "Room " + i;
            }
            else
            {
                do
                {
                    baseRoom = availableRoomList[UnityEngine.Random.Range(0, availableRoomList.Count)];
                }while (baseRoom.availableDirections.Count < 1);

                
                Direction dir = baseRoom.availableDirections[UnityEngine.Random.Range(0, baseRoom.availableDirections.Count)];
                Vector2 newPos = new Vector2(baseRoom.GetXCoord(), baseRoom.GetYCoord()) + Room.DirectionToOffset(dir, distance);

                if (!RoomExistsAt(newPos))
                {
                    GameObject newRoomObj = Instantiate(roomPrefab, newPos, Quaternion.identity);
                    Room newRoom = newRoomObj.GetComponent<Room>();
                    SaveRoom(newRoom, newPos);
                    availableRoomList.Add(newRoom);
                    RemoveDirection(baseRoom, dir);
                    RemoveDirection(newRoom, Opposite(dir));
                    newRoom.GenerateRoomTiles();
                    newRoom.name = "Room " + i;
                    ConnectNeighbors();
                }
                else
                {
                    i--;
                }
            }
        }
        foreach (var room in rooms)
        {
            room.GenerateDoorsAndPaths();            
        }
        
        SamePlace();
    }
    private void ConnectNeighbors()
    {
        foreach (Room room in rooms)
        {
            Vector2Int pos = new Vector2Int(room.GetXCoord(), room.GetYCoord());

            // NORTH
            foreach (Room r in rooms)
            {
                if (r.GetXCoord() == pos.x && r.GetYCoord() == pos.y + distance)
                {
                    room.SetNeighbor(Direction.North, r);
                }
            }

            // SOUTH
            foreach (Room r in rooms)
            {
                if (r.GetXCoord() == pos.x && r.GetYCoord() == pos.y - distance)
                {
                    room.SetNeighbor(Direction.South, r);
                }
            }

            // EAST
            foreach (Room r in rooms)
            {
                if (r.GetXCoord() == pos.x + distance && r.GetYCoord() == pos.y)
                {
                    room.SetNeighbor(Direction.East, r);
                }
            }

            // WEST
            foreach (Room r in rooms)
            {
                if (r.GetXCoord() == pos.x - distance && r.GetYCoord() == pos.y)
                {
                    room.SetNeighbor(Direction.West, r);
                }
            }
        }
    }

    public static Direction Opposite(Direction dir)
    {
        switch (dir)
        {
            case Direction.North: return Direction.South;
            case Direction.South: return Direction.North;
            case Direction.East: return Direction.West;
            case Direction.West: return Direction.East;
            default: return dir;
        }
    }
    public static void RemoveDirection(Room room, Direction dir)
    {
        room.availableDirections.Remove(dir);
        if (room.availableDirections.Count == 0)
        {
            Instance.availableRoomList.Remove(room);
        }
    }
    
    bool RoomExistsAt(Vector2 pos)
    {
        foreach (Room r in rooms)
        {
            if (r.GetXCoord() == pos.x && r.GetYCoord() == pos.y)
                return true;
        }
        return false;
    }

    void SaveRoom(Room room, Vector3 pos)
    {
        room.SetXCoord((int)pos.x);
        room.SetYCoord((int)pos.y);
        rooms.Add(room);
        index++;
    }

    public void SamePlace()
    {
        bool ok = false;
        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = i + 1; j < rooms.Count; j++)
            {
                if (rooms[i].GetXCoord() == rooms[j].GetXCoord() &&
                    rooms[i].GetYCoord() == rooms[j].GetYCoord())
                {
                    ok = true;
                }
            }
        }      
    }
}
