using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "TileScriptableObject", menuName = "ScriptableObjects/Tile")]

public class TileScriptableObject : ScriptableObject
{
    public TileType type;
    public Sprite sprite;
}
