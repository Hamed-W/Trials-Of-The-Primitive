using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;
    private Vector2 moveInput;
    [SerializeField] private float movementSpeed;
    public float gravityVelocity;
    public float gravityMultiplier;

    [SerializeField] private float groundedForce = -2f;
    private float verticalVelocity;

    [SerializeField] private float turnSpeed = 10f;

    public Transform cameraPos;
    public CameraManager cameraManager;

    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundRayDistance = 1.5f;

    private PlayerAnimation playerAnimScript;

    private bool canJump = false;
    public float jumpForce = 10f;

    [SerializeField] private float fallingVelocityThreshold;

    public float sprintMultiplier;
    private bool isSprinting = false;

    [SerializeField] private PlayerStats playerStats;

    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private AudioManager audioManager;

    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.32f;

    private float stepTimer;


    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerAnimScript = GetComponent<PlayerAnimation>();
        playerStats.StatsChanged += UpdateMovementSpeed;
    }

    // Start is called before the first frame update
    void Start()
    {
        moveInput = Vector2.zero;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        HandleMovement();
        HandleFootstepsSound();
    }

    void LateUpdate()
    {
        RotatePlayer();
    }


    private void UpdateMovementSpeed()
    {
        if (isSprinting)
        {
            movementSpeed /= sprintMultiplier;
            movementSpeed = playerStats.movementSpeed;
            movementSpeed *= sprintMultiplier;
        }
        else
        {
            movementSpeed = playerStats.movementSpeed;
        }
    }

    void HandleMovement()
    {
        Vector3 cameraForward = cameraPos.forward;
        Vector3 cameraRight = cameraPos.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = cameraRight * moveInput.x + cameraForward * moveInput.y;

        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

        Vector3 rayOrigin = transform.position + Vector3.up * 0.2f;

        bool foundGround = Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, groundRayDistance, groundMask);

        if (foundGround)
        {
            moveDirection = Vector3.ProjectOnPlane(moveDirection,hit.normal);

            // ProjectOnPlane slightly changes magnitude
            moveDirection = Vector3.ClampMagnitude(moveDirection,moveInput.magnitude);
        }

        if (characterController.isGrounded)
        {
            // Keeps the character controller pressed against the ground.
            verticalVelocity = groundedForce;

            if (canJump)
            {
                verticalVelocity += jumpForce;
            }
        }
        else
        {
            verticalVelocity -= gravityVelocity * gravityMultiplier * Time.deltaTime;
            canJump = false;
        }

        Vector3 velocity = moveDirection * movementSpeed;

        velocity.y += verticalVelocity;

        characterController.Move(velocity * Time.deltaTime);

        AnimState finalAnimation = (characterController.velocity.y < -fallingVelocityThreshold
        ? AnimState.Falling
        : (moveInput.sqrMagnitude > 0.001f
            ? (isSprinting == true? AnimState.Running : AnimState.Walking)
            : AnimState.Idle));

        playerAnimScript.SetAnimState(finalAnimation);
    }

    private void HandleFootstepsSound()
    {
        if (!characterController.isGrounded || moveInput.sqrMagnitude < 0.01f)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;
        if (stepTimer > 0f) return;

        Biome biome = mapGenerator.GetBiomeFromCoord(transform.position);
        if (biome == Biome.None) biome = Biome.Grass;
        audioManager.PlayFootstep(biome);

        stepTimer = isSprinting ? runStepInterval : walkStepInterval;
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        if (characterController.isGrounded)
        {
            canJump = true;
        }
    }

    void OnSprint(InputValue value)
    {
        if (value.isPressed)
        {
            if (isSprinting) return;
            movementSpeed *= sprintMultiplier;
            isSprinting = true;
        }
        else
        {
            movementSpeed /= sprintMultiplier;
            isSprinting = false;
        }
    }

    void RotatePlayer()
    {
        bool isThirdPerson = cameraManager.isThirdPerson;

        
        if (cameraManager.IsBlending)
            return;

        // In third person mode we only rotate character when player intends to move.
        // In first person mode we always rotate character where player is looking.
        if (isThirdPerson && !cameraManager.IsLockedOn && moveInput.magnitude <= 0.01f)
            return;

        float finalYaw = cameraManager.CalculateYaw(moveInput);

        transform.rotation = Quaternion.Euler(0f, finalYaw, 0f);
    }
}
