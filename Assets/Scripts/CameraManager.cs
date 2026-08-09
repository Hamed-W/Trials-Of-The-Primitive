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
    public Key cameraLockKey = Key.E;

    public float sensitivity = 1f;


    [SerializeField] private Transform player;
    [SerializeField] private Transform mainCamera;

    [SerializeField] private CinemachineBrain cinemachineBrain;

    public bool IsBlending => cinemachineBrain != null && cinemachineBrain.IsBlending;

    public float yawToRotate = 0;

    [SerializeField] private float turnSpeed = 10f;

    [Header("Lock On")]
    [SerializeField] private CinemachineVirtualCamera lockOnCamera;

    [SerializeField] private CinemachineTargetGroup lockOnTargetGroup;

    [SerializeField] private float lockOnTurnSpeed = 12f;

    [SerializeField] private Transform lockedTarget;

    public bool IsLockedOn => lockedTarget != null;

    [SerializeField] private Transform testEnemy;

    [SerializeField] private Transform thirdPersonTarget;

    [SerializeField] private float lockOnDistance = 20f;
    [SerializeField] private float lockOnRadius = 5f;

    private List<Transform> lockOnTargets = new List<Transform>();
    [SerializeField] private int currentTargetIndex = -1;

    [SerializeField] private float switchTargetThreshold = 2f;
    [SerializeField] private bool canSwitchTarget = true;

    [SerializeField] private float targetRefreshRate = 0.2f;
    private float targetRefreshTimer;




    // Start is called before the first frame update
    void Start()    
    {
        /*Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;*/
        pov = virtualCam.GetCinemachineComponent<CinemachinePOV>();
        
        ApplySensitivity();
    }

    // Update is called once per frame
    void Update()
    {
        CheckInputs();
        if (currentTargetIndex != -1) // IsLockedOn will be false if there is no locked on target.
        {
            targetRefreshTimer -= Time.deltaTime;

            if (targetRefreshTimer <= 0f)
            {
                if (!IsBlending) UpdateTargetList();
                targetRefreshTimer = targetRefreshRate;
            }

            if (IsLockedOn) // Should only try switch targets if there are any targets.
            {
                TargetSwitchInput();
            }
        }
    }


    public float CalculateYaw(Vector2 moveInput)
    {
        if (!isThirdPerson)
        {
            return virtualCam.State.FinalOrientation.eulerAngles.y;
        }


        float targetYaw;

        if (IsLockedOn)
        {
            Vector3 targetDirection = lockedTarget.position - player.position;

            targetDirection.y = 0f;

            if (targetDirection.sqrMagnitude > 0.001f)
            {
                targetYaw = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;

                return Mathf.LerpAngle(player.eulerAngles.y, targetYaw, lockOnTurnSpeed * Time.deltaTime);
            }

            return player.eulerAngles.y;
        }

        float cameraYaw = freeLookCamera.State.FinalOrientation.eulerAngles.y;

        float offsetDeg = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg;

        targetYaw = cameraYaw + offsetDeg;

        return Mathf.LerpAngle(player.eulerAngles.y,targetYaw,turnSpeed * Time.deltaTime);
    }


    void CheckInputs()
    {
        /*
        // Cursor lock
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }*/

        // Camera toggle
        if (Keyboard.current != null && Keyboard.current[toggleCameraKey].wasPressedThisFrame)
        {
            if (IsLockedOn) UnlockTarget();
            ToggleView();
        }
        if (Keyboard.current != null && Keyboard.current[cameraLockKey].wasPressedThisFrame)
        {
            if (IsLockedOn) UnlockTarget();
            else
            {
                FindLockOnTargets();

                if (lockOnTargets.Count > 0)
                {
                    currentTargetIndex = 0;
                    LockOntoTarget(lockOnTargets[0]);
                }
            }
        }
    }

    public void ApplySensitivity()
    {
        lookAction.action.ApplyBindingOverride(new InputBinding{overrideProcessors =$"scaleVector2(x={sensitivity},y={sensitivity})"});
    }


    private void ToggleView()
    {
        if (cinemachineBrain.IsBlending)
            return;

        /*
        if (IsLockedOn)
        {
            lockedTarget = null;
            lockOnCamera.Priority = 5;
            isThirdPerson = false;
        }*/

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

    private void ConfigureLockOnGroup(Transform enemy)
    {
        lockOnTargetGroup.m_Targets = new CinemachineTargetGroup.Target[] 
        { 
            new CinemachineTargetGroup.Target {
                target = thirdPersonTarget, 
                weight = 1f, radius = 1f
                },
            new CinemachineTargetGroup.Target {
                target = enemy,
                weight = 1f,
                radius = 1f
                }
        };
    }

    public void LockOntoTarget(Transform target)
    {
        if (target == null)
            return;

        if (IsBlending)
            return;

        lockedTarget = target;

        ConfigureLockOnGroup(target);

        // Lock-on is a third-person state.
        isThirdPerson = true;

        lockOnCamera.PreviousStateIsValid = false;

        virtualCam.Priority = 5;
        freeLookCamera.Priority = 10;
        lockOnCamera.Priority = 30;
    }


    private void FindLockOnTargets()
    {
        lockOnTargets.Clear();
        Vector3 direction = mainCamera.forward;

        RaycastHit[] hits = Physics.SphereCastAll(player.position,lockOnRadius, direction, lockOnDistance);

        foreach (RaycastHit hit in hits)
        {
            EntityBehaviour entity = hit.collider.GetComponentInParent<EntityBehaviour>();

            if (entity == null) continue;

            Transform target = entity.transform;

            Vector3 directionToTarget = (target.position - player.position).normalized;

            if (Vector3.Dot(mainCamera.forward, directionToTarget) <= 0f) continue; // Direction vector player -> target should be forward, in line with camera.forward as a result. If dot product is < 0 then enemy is behind the player.

            if (!lockOnTargets.Contains(target))
            {
                lockOnTargets.Add(target);
            }
        }
        int SortTargets(Transform a, Transform b)
        {
            float distanceA = (a.position - player.position).sqrMagnitude;
            float distanceB = (b.position - player.position).sqrMagnitude;
            return distanceA.CompareTo(distanceB);
        }
        lockOnTargets.Sort((a, b) => SortTargets(a,b));
    }

    private void UnlockTarget()
    {
        lockedTarget = null;

        currentTargetIndex = -1;
        lockOnTargets.Clear();

        freeLookCamera.m_XAxis.Value = 0f;
        freeLookCamera.PreviousStateIsValid = false;

        lockOnCamera.Priority = 5;

        isThirdPerson = true;

        virtualCam.Priority = 10;
        freeLookCamera.Priority = 20;

        Debug.Log("Unlocked");
    }

    private void UpdateTargetList()
    {
        Debug.Log("Getting new list");
        Transform currentTarget = lockedTarget;

        FindLockOnTargets();

        if (currentTarget != null && !lockOnTargets.Contains(currentTarget))
        {
            lockOnTargets.Add(currentTarget);
            currentTargetIndex = lockOnTargets.IndexOf(currentTarget);
        }
        else
        {
            if (currentTarget == null && lockOnTargets.Count > 0)
            {
                Debug.Log("New target");
                currentTargetIndex = 0;
                LockOntoTarget(lockOnTargets[currentTargetIndex]);
            }
        }

        if (lockOnTargets.Count == 0)
        {
            Debug.Log("Unlock Target");
            UnlockTarget();
        }
    }

    private void TargetSwitchInput()
    {
        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

        if (lookInput.x > switchTargetThreshold && canSwitchTarget)
        {
            Debug.Log("Right");
            SwitchTarget(1); // Look to the right index
            canSwitchTarget = false;
        }
        else if (lookInput.x < -switchTargetThreshold && canSwitchTarget)
        {
            Debug.Log("Left");
            SwitchTarget(-1); // Look to the left index
            canSwitchTarget = false;
        }

        // Return mouse to rest to be able to switch again.
        if (Mathf.Abs(lookInput.x) < 0.01)
        {
            canSwitchTarget = true;
        }
    }
    private void SwitchTarget(int direction)
    {
        if (lockOnTargets.Count <= 1) return;

        currentTargetIndex += direction;

        if (currentTargetIndex >= lockOnTargets.Count)
        {
            currentTargetIndex = 0; // Return to closest enemy
        }
        else if (currentTargetIndex < 0)
        {
            currentTargetIndex = lockOnTargets.Count - 1; //Go to furthest enemy.
        }

        LockOntoTarget(lockOnTargets[currentTargetIndex]);
    }
}
