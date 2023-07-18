using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public static PlayerCamera instance;

    public Camera cameraObject;
    public PlayerManager player;
    [SerializeField] Transform cameraPivotTransform;
    // change to inpact camera performace
    [Header("Camera Settings")]
    private float cameraSmoothSpeed = 1; // the bigger the number the longer the camera to reach its position during movememnt
    [SerializeField] float leftAndRightRotationSPeed = 220;
    [SerializeField] float upAndDownRotationSpeed = 220;
    [SerializeField] float minimumPivot = -30;//lowest point looking down 
    [SerializeField] float maximumPivot = 60; // highest point looking up
    [SerializeField] float cameraCollisionRadius = 0.2f;
    [SerializeField] LayerMask collideWithLayers;

    [Header("Camera Values")]
    private Vector3 cameraVelocity;
    private Vector3 cameraObjectPosition; // moves camera if colides 
    [SerializeField] float leftAndRightLookAngle;
    [SerializeField] float upAndDownLookAngle;
    private float cameraZPosition; // value used for camera collisions
    private float targetCameraZPosition; // calue used for camera collisions

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
        cameraZPosition = cameraObject.transform.localPosition.z;
    }
    public void HandleAllCameraActions()
    {
        if (player != null)
        {
            //Follow the player
            HandleFollowTarget();
            //rotate around player
            HandleRotation();
            //collide with objects
            HandleCollisions();
        }

    }

    private void HandleFollowTarget()
    {
        Vector3 targetCameraPosition = Vector3.SmoothDamp
            (transform.position, 
            player.transform.position, 
            ref cameraVelocity, cameraSmoothSpeed * Time.deltaTime);
        transform.position = targetCameraPosition;
    }
    private void HandleRotation()
    {
        // if locked on force rotation
        //else rotate regularly

        // normal rotations
        //rotate left and right based on horizontal movement on the right jpystick
        leftAndRightLookAngle += (PlayerInputManager.instance.cameraHorizontalInput * leftAndRightRotationSPeed) * Time.deltaTime;
        //rotate up and down based on vertical movement on the right joystick
        upAndDownLookAngle -= (PlayerInputManager.instance.cameraVerticalInput * upAndDownRotationSpeed) * Time.deltaTime;
        //clamp the up and down look agle between a min and max
        upAndDownLookAngle = Mathf.Clamp(upAndDownLookAngle, minimumPivot, maximumPivot);

        Vector3 cameraRotation = Vector3.zero;
        Quaternion targetRotation;

        //rotate this gameobject left and right 
        cameraRotation.y = leftAndRightLookAngle;
        targetRotation = Quaternion.Euler(cameraRotation);
        transform.rotation = targetRotation;

        //rotate the pivot gameobject up and down
        cameraRotation = Vector3.zero;
        cameraRotation.x = upAndDownLookAngle;
        targetRotation = Quaternion.Euler(cameraRotation);
        cameraPivotTransform.localRotation = targetRotation;
    }

    private void HandleCollisions()
    {
        targetCameraZPosition = cameraZPosition;
        RaycastHit hit;
        //direction for collision check
        Vector3 direction = cameraObject.transform.position - cameraPivotTransform.position;
        direction.Normalize();

        // we check if there is an object infrom of our camera in desired direction
        if (Physics.SphereCast(cameraPivotTransform.position, cameraCollisionRadius, direction, out hit, Mathf.Abs(targetCameraZPosition), collideWithLayers))
        {
            // get distance from it
            float distanceFromHitObject = Vector3.Distance(cameraPivotTransform.position, hit.point);
            targetCameraZPosition = -(distanceFromHitObject - cameraCollisionRadius);

            //if our target position is less than our collision radius
            if (Mathf.Abs(targetCameraZPosition) < cameraCollisionRadius)
            {
                targetCameraZPosition = -cameraCollisionRadius;
            }
        }
        cameraObjectPosition.z = Mathf.Lerp(cameraObject.transform.localPosition.z, targetCameraZPosition, 0.2f);
        cameraObject.transform.localPosition = cameraObjectPosition;
    }
}
