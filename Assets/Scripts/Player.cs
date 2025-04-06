using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    int groundLayer;
    public float rayRange = 50f;
    // Start is called before the first frame update
    void Start()
    {
        groundLayer = LayerMask.GetMask("Ground");
    }

    // Update is called once per frame
    void Update()
    {
        if (InputManager.Instance.GetPlayerMouseInput())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit,1000f,groundLayer))
            {
                Vector3 pos = hit.collider.GetComponent<Transform>().position;
                Debug.Log("Kattintás pozíciója: " + hit.point);
                transform.position = pos;
                Camera.main.transform.position = hit.collider.GetComponentInParent<Room>().CameraPlacement;
            }
        }
        
    }
}
