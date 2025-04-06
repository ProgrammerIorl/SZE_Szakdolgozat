using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class MapGenerator : MonoBehaviour
{
    public Dictionary<int, Room> rooms = new Dictionary<int, Room>();
    public GameObject prefab;
    public int count = 3;
    int distance = 10;
    int index = 0;

    void Start()
    {
        StartCoroutine(RoomGeneration());
    }

    IEnumerator RoomGeneration()
    {


        while (index < count)
        {
            Vector3 position = NewPosition();
            GameObject roomObject = Instantiate(prefab, position, Quaternion.identity);
            Room room = roomObject.GetComponent<Room>();
            SaveRoomCoords(room, position);
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(RoomGeneration());
            yield return new WaitForSeconds(0.5f);

        }
    }


    Vector3 NewPosition()
    {
        Vector3 newPosition = Vector3.zero;

        if (rooms.Count > 0)
        {
            Room room = GetLastRoom();
            newPosition = new Vector3(room.GetXCoord(), 0, room.GetZCoord());
            bool isNotValid;
            do
            {
                isNotValid = false;
                switch (Random.Range(1, 4))
                {
                    case 1:
                        newPosition.x += distance;
                        break;
                    case 2:
                        newPosition.x -= distance;
                        break;
                    case 3:
                        newPosition.z += distance;
                        break;
                    
                }
                for (int i = 0; i < rooms.Count; i++)
                {
                    if (rooms[i].GetXCoord() == newPosition.x && rooms[i].GetZCoord() == newPosition.z)
                    {
                        isNotValid = true;
                        break;
                    }
                }
            } while (isNotValid);
        }
        return newPosition;
    }

    Room GetLastRoom()
    {
        return rooms.Last().Value;
    }
    void SaveRoomCoords(Room room, Vector3 position)
    {
        room.SetXCoord(position.x);
        room.SetZCoord(position.z);
        rooms.Add(index, room);
        index++;
    }

}
