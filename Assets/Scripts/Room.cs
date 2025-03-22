using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    private float xcoord;
    private float zcoord;
    public float GetXCoord() { return xcoord; }
    public float GetZCoord() { return zcoord; }
    public void SetXCoord(float setx) {  xcoord=setx; }
    public void SetZCoord(float setz) { xcoord = setz; }
}
