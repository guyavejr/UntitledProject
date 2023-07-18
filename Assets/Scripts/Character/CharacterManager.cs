using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CharacterManager : NetworkBehaviour
{
    [HideInInspector] public CharacterController characterController;
    [HideInInspector] public Animator animator;

    [HideInInspector] public CharacterNetworkManager characterNetworkManager;

    [Header("Flag")]
    public bool isPerformingAction = false;
    public bool applyRootMotion = false;
    public bool canRotate = true;
    public bool canMove = true;

    protected virtual void Awake()
    {
        DontDestroyOnLoad(this);

        characterController = GetComponent<CharacterController>();
        characterNetworkManager = GetComponent<CharacterNetworkManager>();
        animator = GetComponent<Animator>();
    }

    protected virtual void Update()
    {
        //if character is being controlled form our side, then assign its network position of our transform
        if (IsOwner)
        {
            characterNetworkManager.networkPosition.Value = transform.position;
            characterNetworkManager.networkRotation.Value = transform.rotation;
        }
        //
        else
        {
            //position
            transform.position = Vector3.SmoothDamp(transform.position,
                characterNetworkManager.networkPosition.Value,
                ref characterNetworkManager.networkPostitionVelocity,
                characterNetworkManager.networkPositionSmoothTime);
            //rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, 
                characterNetworkManager.networkRotation.Value, 
                characterNetworkManager.networkRotationSmoothTime);
        }
    }
   
    protected virtual void LateUpdate()
    {

    }
}
