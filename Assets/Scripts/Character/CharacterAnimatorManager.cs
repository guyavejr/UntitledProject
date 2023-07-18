using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CharacterAnimatorManager : MonoBehaviour
{
    CharacterManager character;
    int vertical;
    int horizontal;

protected virtual void Awake()
    {
        character = GetComponent<CharacterManager>();

        vertical = Animator.StringToHash("Vertical");
        horizontal = Animator.StringToHash("Horizontal");
    }
    public void UpdateAnimatorMovementParameters(float horizontalMovement, float verticalMovement, bool isSprinting)
    {
        float horizontalAmount = horizontalMovement;
        float verticalAmount = verticalMovement;

        if (isSprinting)
        {
            verticalAmount = 2;
        }
        //0.1f. time.deltatime blends the animation switching 
        character.animator.SetFloat(horizontal, horizontalMovement, 0.1f, Time.deltaTime);
        character.animator.SetFloat(vertical, verticalMovement, 0.1f, Time.deltaTime);
    }

    public virtual void PlayerTargetActionAnimation(
        string targetAnimation, 
        bool isPerformingAction, 
        bool applyRootMotion = true, 
        bool canRotate = false, 
        bool canMove = false)
    {
        character.applyRootMotion = applyRootMotion;
        character.animator.CrossFade(targetAnimation, 0.2f);
        // can be used to stop character from attempting new action
        // turn true if you are stunned
        // check before attempting action
        character.isPerformingAction = isPerformingAction;
        character.canMove = canMove;
        character.canRotate = canRotate;

        //tell the server/host that animation has been played
        character.characterNetworkManager.NotifyServerOfActionAnimationServerRpc(
            NetworkManager.Singleton.LocalClientId, targetAnimation, applyRootMotion);
    }
}
