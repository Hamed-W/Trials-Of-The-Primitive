using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;


public class CameraManager : MonoBehaviour
{
    public InputActionReference lookAction;

    public bool isThirdPerson = false;

    [SerializeField] private CinemachineFreeLook freeLookCamera;
    [SerializeField] private CinemachineVirtualCamera virtualCam;
    private CinemachinePOV pov;
    
    [Header("InputKeys")]
    public Key toggleCameraKey = Key.Q;

    public float sensitivity = 1f;



    [SerializeField] private Transform player;
    [SerializeField] private Transform mainCamera;

    [SerializeField] private CinemachineBrain cinemachineBrain;

    public bool isBlending => cinemachineBrain != null && cinemachineBrain.IsBlending;

    public float yawToRotate = 0;

    [SerializeField] private float turnSpeed = 10f;


    // Start is called before the first frame update
    void Start()    
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        pov = virtualCam.GetCinemachineComponent<CinemachinePOV>();
        
        ApplySensitivity();
    }

    // Update is called once per frame
    void Update()
    {
       CheckInputs();
    }


    public float CalculateYaw(Vector2 moveInput)
    {
        if (!isThirdPerson)
        {
            return virtualCam.State.FinalOrientation.eulerAngles.y;
        }

        float cameraYaw = freeLookCamera.State.FinalOrientation.eulerAngles.y;

        float offsetDeg = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg;

        float targetYaw = cameraYaw + offsetDeg;

        return Mathf.LerpAngle(player.eulerAngles.y,targetYaw,turnSpeed * Time.deltaTime);
    }


    void CheckInputs()
    {
        // Cursor lock
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Camera toggle
        if (Keyboard.current != null && Keyboard.current[toggleCameraKey].wasPressedThisFrame) ToggleView();
    }

    public void ApplySensitivity()
    {
        lookAction.action.ApplyBindingOverride(new InputBinding{overrideProcessors =$"scaleVector2(x={sensitivity},y={sensitivity})"});
    }


    private void ToggleView()
    {
        if (cinemachineBrain.IsBlending)
            return;

        isThirdPerson = !isThirdPerson;

        if (isThirdPerson)
        {
            freeLookCamera.m_XAxis.Value = 0f;
            // The reset y is minor (try without it). It causes the slight movement after switching (if no input) but ensures rotation is consistent after swapping to third person.
            freeLookCamera.m_YAxis.Value = 0f;
            freeLookCamera.PreviousStateIsValid = false;

            virtualCam.Priority = 10;
            freeLookCamera.Priority = 20;
        }
        else
        {
        if (pov != null)
        {
            // Face horizontally in the player's current direction.
            pov.m_HorizontalAxis.Value = player.eulerAngles.y;

            // Reset looking up/down so the camera faces forward.
            pov.m_VerticalAxis.Value = 0f;
        }

            virtualCam.PreviousStateIsValid = false;

            freeLookCamera.Priority = 10;
            virtualCam.Priority = 20;
        }
    }
}
