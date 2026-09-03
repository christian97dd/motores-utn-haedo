using UnityEngine;

public class WASDMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float runSpeed = 8f;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(horizontal, 0f, vertical).normalized;

       
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        
        bool isCrouching = Input.GetKey(KeyCode.LeftControl);

        
        float currentSpeed = isRunning ? runSpeed : speed;

        
        transform.position += movement * currentSpeed * Time.deltaTime;

        
        if (movement != Vector3.zero)
        {
            transform.forward = movement;
        }

        
        animator.SetFloat("movement", movement.magnitude);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isCrouching", isCrouching);
    }
}