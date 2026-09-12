using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    public InputActionAsset inputActions;
    public CharacterController playerCharacterController;

    private InputAction playerMoveAction;
    private InputAction playerSprintAction;
    private InputAction playerCrouchAction;

    [SerializeField] private Transform playerCamera;
    private Vector2 playerMoveAmount;

    private Animator playerAnimator;

    private float playerWalkSpeed = 5f;
    private float playerRunSpeed = 8f;
    private float playerRotateDampening = 0.1f;
    private float turnSmoothingVelocity;

    [SerializeField] private float playerGravity = -9.8f;
    private float _verticalVelocity;
    private float groundedVerticalVelocity = -2f;

    [Header("Detección de suelo")]
    [SerializeField] private Transform groundCheckPivot;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    private void OnEnable()
    {
        inputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        inputActions.FindActionMap("Player").Disable();
    }

    private void Awake()
    {
        playerMoveAction = InputSystem.actions.FindAction("Move");
        playerSprintAction = InputSystem.actions.FindAction("Sprint");
        playerCrouchAction = InputSystem.actions.FindAction("Crouch");
        playerAnimator = GetComponentInChildren<Animator>();
    }
    private void Update()
    {
        playerMoveAmount = playerMoveAction.ReadValue<Vector2>();
        PlayerMoveAndRotate();
    }

    private void PlayerMoveAndRotate()
    {
        Vector3 playerDirection = new Vector3(playerMoveAmount.x, 0f, playerMoveAmount.y).normalized;

        bool isRunning = playerSprintAction != null && playerSprintAction.IsPressed();
        bool isCrouching = playerCrouchAction != null && playerCrouchAction.IsPressed();

        Vector3 horizontalMove = Vector3.zero;

        if (playerDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(playerDirection.x, playerDirection.z) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;
            float smoothTargetAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothingVelocity, playerRotateDampening);

            transform.rotation = Quaternion.Euler(0f, smoothTargetAngle, 0f);

            float currentSpeed = isRunning ? playerRunSpeed : playerWalkSpeed;

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            horizontalMove = moveDirection.normalized * currentSpeed;
        }

        ApplyGravity();

        Vector3 finalMove = horizontalMove + Vector3.up * _verticalVelocity;
        playerCharacterController.Move(finalMove * Time.deltaTime);

        UpdatePlayerAnimator(playerDirection.magnitude, isRunning, isCrouching);
    }

    private void ApplyGravity()
    {
        if (IsGrounded() && _verticalVelocity < 0f)
        {
            _verticalVelocity = groundedVerticalVelocity;
        }
        else
        {
            _verticalVelocity += playerGravity * Time.deltaTime;
        }
    }

    private bool IsGrounded()
    {
        return Physics.CheckSphere(GetGroundCheckOrigin(), groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);
    }

    private Vector3 GetGroundCheckOrigin()
    {
        return groundCheckPivot != null ? groundCheckPivot.position : transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsGrounded() ? Color.green : Color.red;
        Gizmos.DrawWireSphere(GetGroundCheckOrigin(), groundCheckRadius);
    }

    private void UpdatePlayerAnimator(float movement, bool isRunning, bool isCrouching)
    {
        if (playerAnimator == null) return;

        playerAnimator.SetFloat("movement", movement);
        playerAnimator.SetBool("isRunning", isRunning);
        playerAnimator.SetBool("isCrouching", isCrouching);
    }
}
