using UnityEngine;
using UnityEngine.InputSystem;

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

        if (playerDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(playerDirection.x, playerDirection.z) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;
            float smoothTargetAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothingVelocity, playerRotateDampening);

            transform.rotation = Quaternion.Euler(0f, smoothTargetAngle, 0f);

            float currentSpeed = isRunning ? playerRunSpeed : playerWalkSpeed;

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            playerCharacterController.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);
        }

        // Fuera del if, si no al soltar el joystick el Animator se queda en la animación de caminar
        UpdatePlayerAnimator(playerDirection.magnitude, isRunning, isCrouching);
    }

    private void UpdatePlayerAnimator(float movement, bool isRunning, bool isCrouching)
    {
        if (playerAnimator == null) return;

        playerAnimator.SetFloat("movement", movement);
        playerAnimator.SetBool("isRunning", isRunning);
        playerAnimator.SetBool("isCrouching", isCrouching);
    }
}
