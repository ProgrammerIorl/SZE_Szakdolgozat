
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MapGenerator : MonoBehaviour
{
    public Dictionary<int,Room> rooms= new Dictionary<int, Room>();
    public GameObject prefab;
    // Start is called before the first frame update
    public int count = 100;
    int index = 0;


    void Start()
    {
        
        StartCoroutine(RoomGeneration());
        Debug.Log(rooms.Count);
    }

    IEnumerator RoomGeneration()
    {   
        if (index >= count) yield break;
        Vector3 position = NewPosition();
        GameObject roomObject =Instantiate(prefab, position, transform.rotation);
        Room room = roomObject.GetComponent<Room>();
        if (rooms.Count != 0)
        {
            room.SetXCoord(position.x);
            room.SetZCoord(position.z);
        }
        else 
        {
            Debug.Log("ELSE");
            room.SetXCoord(0);
            room.SetZCoord(0);
        }
        
        rooms.Add(index,room);
        index++;
        int doors = Random.Range(1, 4);
        for (int i = 0; i < doors; i++)
        {
            yield return new WaitForSeconds(1f);
            StartCoroutine(RoomGeneration());
        }
        
    }
    Vector3 NewPosition() 
    {
        Vector3 newPosition = Vector3.zero;
        if (rooms.Count != 0)
        {
            Room room = GetLastRoom();
            Debug.Log(" X: "+GetLastRoom().GetXCoord()+" Z:"+GetLastRoom().GetZCoord());
            newPosition = new(room.GetXCoord(),0,room.GetZCoord());
            switch (Random.Range(1, 4))
            {
                
                case 1:
                    newPosition.x++;
                    
                    break;
                case 2:
                    newPosition.x--;
                    
                    break;
                case 3:
                    newPosition.z++;
                    
                    break;
                default:
                    
                    break;
            }
        }
        Debug.Log(newPosition);

        return newPosition;
    }

    Room GetLastRoom()
    {
        return rooms.Last().Value;
    }

}
