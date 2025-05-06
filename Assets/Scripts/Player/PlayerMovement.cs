using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;

    [Header("Налаштування руху")]
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float gravityForce = -20f;
    [SerializeField] private float jumpHeight = 3f;

    [Header("Перевірка на землю")]
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;

    private bool isGrounded;
    private Vector3 velocity;

    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    private readonly KeyCode jumpKey = KeyCode.Space;
    private readonly KeyCode sprintKey = KeyCode.LeftShift;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
        HandleJumpAndGravity();
    }

    private void HandleMovement()
    {
        float xInput = Input.GetAxis("Horizontal");
        float zInput = Input.GetAxis("Vertical");

        Vector3 move = transform.right * xInput + transform.forward * zInput;
        move.y = 0f;

        float currentSpeed = moveSpeed;

        if (Input.GetKey(sprintKey))
        {
            currentSpeed *= sprintMultiplier;
        }

        Vector3 moveVector = move * currentSpeed;

        if (moveVector.magnitude > currentSpeed)
            moveVector = moveVector.normalized * currentSpeed;

        characterController.Move(moveVector * Time.deltaTime);
    }

    private void HandleJumpAndGravity()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            if (velocity.y < 0f)
            {
                velocity.y = -2f;
            }
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetKeyDown(jumpKey) && coyoteTimeCounter > 0f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityForce);
            coyoteTimeCounter = 0f;
        }

        if (!isGrounded)
        {
            velocity.y += gravityForce * Time.deltaTime;
        }

        characterController.Move(velocity * Time.deltaTime);
    }


}
