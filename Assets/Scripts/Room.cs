using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public float xcoord;
    public float zcoord;
    public float GetXCoord() { return this.xcoord; }
    public float GetZCoord() { return this.zcoord; }
    public void SetXCoord(float setx) {  this.xcoord=setx; }
    public void SetZCoord(float setz) { this.xcoord = setz; }

}
