using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    private float xcoord;
    private float zcoord;
    public Vector3 CameraPlacement;

    public float GetXCoord() { return this.xcoord; }
    public float GetZCoord() { return this.zcoord; }
    public void SetXCoord(float setx) {  this.xcoord=setx; }
    public void SetZCoord(float setz) { this.zcoord = setz; }
    public GameObject tilePrefab;
    public float spacing = 1f;
    /*void Start()
    {
        
        for (int x = 0; x < 10; x++)
        {
            for (int z = 0; z < 10; z++)
            {
                Vector3 pos = new Vector3(x * spacing, 0, z * spacing);
                GameObject tile = Instantiate(tilePrefab, pos, tilePrefab.transform.rotation, transform);
                if ((x + z) % 2 == 0)
                {
                    tile.GetComponent<Renderer>().material.color = Color.white;
                }
                else { tile.GetComponent<Renderer>().material.color = Color.black; }
                
            }
        }
    }*/
    private void Start()
    {
        CameraPlacement = new(transform.position.x + 5, transform.position.y + 15, transform.position.z + 5);
    }

}
