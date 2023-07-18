using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotionManager : CharacterLocomotionManager
{
    PlayerManager player;

    [HideInInspector] public float verticalMovement;
    [HideInInspector] public float horizontalMovement;
    [HideInInspector] public float moveAmount;

    [Header("Movement Settings")]
    private Vector3 moveDirection;
    private Vector3 targetRotationDirection;
    [SerializeField] float walkingSpeed = 2;
    [SerializeField] float runningSpeed = 5;
    [SerializeField] float sprintingSpeed = 15f;
    [SerializeField] float rotationSpeed = 15;

    [Header("Dodge")]
    private Vector3 rollDirection; 

    protected override void Awake()
    {
        base.Awake();

        player = GetComponent<PlayerManager>();
    }

    protected override void Update()
    {
        base.Update();
        if (player.IsOwner)
        {
            player.characterNetworkManager.verticalMovement.Value = verticalMovement;
            player.characterNetworkManager.horizontalMovement.Value = horizontalMovement;
            player.characterNetworkManager.moveAmount.Value = moveAmount;
        }
        else
        {
            moveAmount = player.characterNetworkManager.moveAmount.Value;
            verticalMovement = player.characterNetworkManager.verticalMovement.Value;
            horizontalMovement = player.characterNetworkManager.horizontalMovement.Value;

            //if not locked on pass these values
            player.playerAnimatorManager.UpdateAnimatorMovementParameters(0, moveAmount, 
                player.playerNetworkManager.isSprinting.Value);

            //if locked on pass horizontal values for strafe
        }
    }
    
    public void HandleAllMovement()
    {
        //grounded movement 
        HandleGroundedMovement();
        HandleRotation();
        //aerial movement

    }
    
    private void GetMovementValues()
    {
        verticalMovement = PlayerInputManager.instance.verticalInput;
        horizontalMovement = PlayerInputManager.instance.horizontalInput;
        moveAmount = PlayerInputManager.instance.moveAmount;
        //clamp the movement

    }
    
    private void HandleGroundedMovement()
    {
        if (!player.canMove)
            return;

        GetMovementValues();
        
        moveDirection = PlayerCamera.instance.transform.forward * verticalMovement;
        moveDirection = moveDirection + PlayerCamera.instance.transform.right * horizontalMovement;
        moveDirection.Normalize();
        moveDirection.y = 0;

      
        if (player.playerNetworkManager.isSprinting.Value)
        {
            player.characterController.Move(moveDirection * sprintingSpeed * Time.deltaTime);
            
        }
        else
        {
            if (PlayerInputManager.instance.moveAmount > 0.5f)
            {
                // move at running speed
                player.characterController.Move(moveDirection * runningSpeed * Time.deltaTime);
                
            }
            else if (PlayerInputManager.instance.moveAmount <= 0.5f)
            {
                //walking speed
                player.characterController.Move(moveDirection * walkingSpeed * Time.deltaTime);
                
            }
        }
    }
    
    private void HandleRotation()
    {
        if (!player.canRotate)
            return;
        targetRotationDirection = Vector3.zero;
        targetRotationDirection = PlayerCamera.instance.cameraObject.transform.forward * verticalMovement;
        targetRotationDirection = targetRotationDirection + PlayerCamera.instance.cameraObject.transform.right * horizontalMovement;
        targetRotationDirection.Normalize();
        targetRotationDirection.y = 0;

        if (targetRotationDirection == Vector3.zero)
        {
            targetRotationDirection = transform.forward;
        }

        Quaternion newRotation = Quaternion.LookRotation(targetRotationDirection);
        Quaternion targetRotaion = Quaternion.Slerp(transform.rotation, newRotation, rotationSpeed * Time.deltaTime);
        transform.rotation = targetRotaion;
    }

    public void HandleSprinting()
    {
        if (player.isPerformingAction)
        {
            player.playerNetworkManager.isSprinting.Value = false;
        }
        // if we are out of stamina, set spritning to false
        // if we are moving set sprinting to true
        if (moveAmount > 0.5)
        {
            player.playerNetworkManager.isSprinting.Value = true;
        }
        else
        {
            player.playerNetworkManager.isSprinting.Value = false;
        }
    }
    
    public void AttemptToPerformDodge()
    {
        if (player.isPerformingAction)
            return;
        //if moving attempt roll
        if (PlayerInputManager.instance.moveAmount > 0)
        {
            //perform roll animation
            rollDirection = PlayerCamera.instance.cameraObject.transform.forward * PlayerInputManager.instance.verticalInput;
            rollDirection += PlayerCamera.instance.cameraObject.transform.right * PlayerInputManager.instance.horizontalInput;
            rollDirection.y = 0;
            rollDirection.Normalize();

            Quaternion playerRotation = Quaternion.LookRotation(rollDirection);
            player.transform.rotation = playerRotation;

            player.playerAnimatorManager.PlayerTargetActionAnimation("Roll_Forward_01", true, true);
        }
        //if not moveing perform back step 
        else
        {
            //perform a back step

        }
    }
}
