using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [HideInInspector] public bool canMove = true;
    [HideInInspector] public bool canUseGravity = true;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 7f;
    [SerializeField] private float sprintSpeed = 12f;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float jumpHeight = 10f;
    [SerializeField] private float minInputRegistered = 0.15f;

    [Header("Ground & Gravity")]
    [SerializeField] private float groundCheckDist;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float gravity;
    private bool isGrounded;

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform mainCamera;
    private PlayerAnimController playerAnimController;
    private CharacterController charController;
    private AbilitiesMenu abilitiesMenu;
    private Ascend ascendAbility;
    private PlayerInputMap playerInput;

    private float moveSpeed;
    private float playerHeight;
    private Vector3 velocity;
    private Vector3 moveDir;
    private Quaternion lastTargetRotation;

    //Property to get the player's move direction
    public Vector3 MoveDir { get { return moveDir; } }

    public float PlayerHeight { get { return playerHeight; } }

    private void Awake()
    {
        playerAnimController = GetComponent<PlayerAnimController>();
        charController = GetComponent<CharacterController>();
        abilitiesMenu = GetComponent<AbilitiesMenu>();
        ascendAbility = GetComponent<Ascend>();

        playerInput = new PlayerInputMap();
        playerInput.Enable();

        Cursor.lockState = CursorLockMode.Locked;

        playerInput.PlayerActions.AbilitiesMenu.performed += ctx => ToggleAbilitiesMenu(ctx);
        playerInput.PlayerActions.AbilitiesMenu.canceled += ctx => ToggleAbilitiesMenu(ctx);
        playerInput.PlayerActions.Sprint.performed += ctx => Sprint(ctx);
        playerInput.PlayerActions.Sprint.canceled += ctx => Sprint(ctx);
        playerInput.PlayerActions.Jump.performed += ctx => Jump();
        playerInput.PlayerActions.UseAbility.performed += ctx => UseAbility();
        playerInput.PlayerActions.Cancel.performed += ctx => CancelAbility();

        moveSpeed = walkSpeed;
        playerHeight = charController.height;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (canMove)
            Move();

        if(canUseGravity)
            Gravity();
    }

    private void Update()
    {
        SetLookOrientation();
    }

    private void Move()
    {
        //Get move input vector
        Vector2 moveInput = playerInput.PlayerActions.Movement.ReadValue<Vector2>();

        //If input amount is tiny, reduce it to zero (prevents drift and odd movement)
        if(moveInput.magnitude < minInputRegistered)
            moveInput = Vector2.zero;

        //Set movement direction based on input & camera orientation
        moveDir = (orientation.forward * moveInput.y + orientation.right * moveInput.x).normalized;
       
        //If moving, smoothly rotate player to face move direction
        if (moveDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);

            lastTargetRotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            transform.rotation = lastTargetRotation;
        }      

        //Move player
        charController.Move(moveInput.magnitude * moveDir * moveSpeed * Time.deltaTime);

        //Apply movement animation
        float moveWeight = (moveInput.magnitude * moveSpeed) / sprintSpeed;
        playerAnimController.SetMoveSpeed(moveWeight);
    }

    private void Gravity()
    {
        //Get grounded state
        isGrounded = Physics.CheckSphere(transform.position, groundCheckDist, groundMask);

        //Set velocity to small downward force if on ground
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            playerAnimController.SetJumpingState(false);
        }

        //Apply gravity and move by velocity (y axis)
        velocity.y += gravity * Time.deltaTime;
        charController.Move(velocity * Time.deltaTime);
    }

    private void SetLookOrientation()
    {
        //Set orientation object forward direction
        Vector3 viewDir = transform.position - new Vector3(mainCamera.transform.position.x, transform.position.y, mainCamera.transform.position.z);
        orientation.forward = viewDir.normalized;
    }

    private void Jump()
    {
        if(isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
            playerAnimController.SetJumpingState(true);
        }
    }

    //Attempt to ascend
    private void UseAbility()
    {
        if (ascendAbility.ascendMode)
            ascendAbility.AttemptToAscend();
    }

    //Cancel using ascend mode if using it
    private void CancelAbility()
    {
        if (ascendAbility.ascendMode)
            abilitiesMenu.DectivateAbility();
    }

    private void Sprint(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            if (context.performed && moveDir != Vector3.zero)
            {
                moveSpeed = sprintSpeed;
            }
            else if (context.canceled || moveDir == Vector3.zero)
            {
                moveSpeed = walkSpeed;
            }
        }
        else
        {
            moveSpeed = walkSpeed;
        }
    }

    private void ToggleAbilitiesMenu(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            Cursor.lockState = CursorLockMode.Confined;
            abilitiesMenu.abilitiesCanvas.SetActive(true);
            Time.timeScale = 0f;
        }
        else if(context.canceled)
        {
            Cursor.lockState = CursorLockMode.Locked;
            abilitiesMenu.abilitiesCanvas.SetActive(false);
            Time.timeScale = 1f;
        }

    }
}
