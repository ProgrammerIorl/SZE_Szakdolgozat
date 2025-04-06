using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public class InputManager : MonoBehaviour
{
    private static InputManager _instance;
    private PlayerController PlayerController;
    public Transform cameraTransform;


    public static InputManager Instance
    {
        get
        {
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        PlayerController = new PlayerController();
        
    }

    private void Start()
    {
        cameraTransform = Camera.main.transform;
    }


    void Update()
    {

    }
    private void FixedUpdate()
    { 
    }
    private void OnEnable()
    {
        PlayerController.Enable();

    }
    private void OnDisable()
    {
        PlayerController.Disable();
    }

    public bool GetPlayerMouseInput()
    {
        return PlayerController.PlayerMovement.Movement.triggered;
    }
    



    

    

    

    

    

    




}
