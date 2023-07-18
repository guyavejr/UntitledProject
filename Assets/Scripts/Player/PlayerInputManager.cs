using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager instance;
    public PlayerManager player;
    //Goals
    //Find a way to read the values of joystick 
    //move character based on values 

    PlayerControls playerControls;
    [Header("Camera Movement Input")]
    [SerializeField] Vector2 cameraInput;
    [SerializeField] public float cameraVerticalInput;
    [SerializeField] public float cameraHorizontalInput;

    [Header("Player Movement Input")]
    [SerializeField] Vector2 movementInput;
    [SerializeField] public float verticalInput;
    [SerializeField] public float horizontalInput;
    [SerializeField] public float moveAmount;

    [Header("Player Actions Input")]
    [SerializeField] bool dodgeInput = false;
    [SerializeField] bool sprintInput = false;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        
    }
    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        //when the scene changes run this logic
        SceneManager.activeSceneChanged += OnSceneChange;

        instance.enabled = false;
    }


    private void OnSceneChange(Scene oldScene, Scene newScene)
    {
        //if we are loading into our world scene enable player controler
        if(newScene.buildIndex == WorldSaveGameManager.instance.GetWorldSceneIndex())
        {
            instance.enabled = true;
        }
        // otherwise be at main menu and disable our player 
        // this is so character doesnt move while in menus
        else
        {
            instance.enabled = false;
        }
    }

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();

            playerControls.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();
            playerControls.PlayerCamera.CameraMovement.performed += i => cameraInput = i.ReadValue<Vector2>();
            playerControls.PlayerActions.Dodge.performed += i => dodgeInput = true;
            
            //hold activates and releasing sets false
            playerControls.PlayerActions.Sprint.performed += i => sprintInput = true;
            playerControls.PlayerActions.Sprint.canceled += i => sprintInput = false;

        }
        playerControls.Enable();
    }

    private void OnDestroy()
    {
        // if we destroy this object unsubscribe from event
        SceneManager.activeSceneChanged -= OnSceneChange;
    }

    private void Update()
    {
        HandleAllInputs();
    }

    private void HandleAllInputs()
    {
        HandlePlayerMovementInput();
        HandleCameraMovementInput();
        HandleDodgeInput();
        HandleSpringting();
    }

    private void OnApplicationFocus(bool focus)
    {
        if (enabled)
        {
            if (focus)
            {
                playerControls.Enable();
            }
            else
            {
                playerControls.Disable();
            }
        }
    }

    //Movements

    private void HandlePlayerMovementInput()
    {
        verticalInput = movementInput.y;
        horizontalInput = movementInput.x;

        // returns the absolute value number without a negative sign
        moveAmount = Mathf.Clamp01(Mathf.Abs(verticalInput) + Mathf.Abs(horizontalInput));

        if (moveAmount <= 0.5 && moveAmount > 0)
        {
            moveAmount = 0.5f;
        }
        else if (moveAmount > 0.5 && moveAmount <= 1)
        {
            moveAmount = 1;
        }
        if (player == null)
            return;
        //Pass zero to have non strafing 
        //if we are locked on pass horizontal amount
        player.playerAnimatorManager.UpdateAnimatorMovementParameters(0, moveAmount,
            player.playerNetworkManager.isSprinting.Value);
    }

    private void HandleCameraMovementInput()
    {
        cameraVerticalInput = cameraInput.y;
        cameraHorizontalInput = cameraInput.x;
    }

    //Actions

    private void HandleDodgeInput()
    {
        if (dodgeInput)
        {
            dodgeInput = false;
            // return if menu or ui is open 

            player.playerLocomotionManager.AttemptToPerformDodge();
        }
    }

    private void HandleSpringting()
    {
        if (sprintInput)
        {
            //handle sprinting
            player.playerLocomotionManager.HandleSprinting();
        }
        else
        {
            player.playerNetworkManager.isSprinting.Value = false;
        }
    }
}
