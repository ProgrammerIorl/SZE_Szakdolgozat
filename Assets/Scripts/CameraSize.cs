using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSize : MonoBehaviour
{
    public Vector2Int currentRoom;
    public float roomSize = 13f;
    public float tileSize = 1f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        UpdateCamera();
    }

    void UpdateCamera()
    {
        cam.orthographicSize = roomSize / 2f;        
        float aspect = (float)Screen.width / Screen.height;

        if (aspect >= 1f)
        {
            // Szélesebb, mint magas → levágunk bal és jobb oldalon
            float inset = 1f - (1f / aspect);
            cam.rect = new Rect(inset / 2f, 0f, 1f / aspect, 1f);
        }
        else
        {
            // Magasabb, mint széles → levágunk felül és alul
            float inset = 1f - aspect;
            cam.rect = new Rect(0f, inset / 2f, 1f, aspect);
        }
    }
}
