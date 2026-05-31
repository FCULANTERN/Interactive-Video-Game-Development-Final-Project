using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Mouse Look")]
    public Camera mainCamera;
    public LayerMask mouseAimMask;
    public float rotateSpeed = 15f;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private Animator animator;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (jumpAction.action.triggered && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (AchievementManager.Instance != null)
                AchievementManager.Instance.RegisterJump();

        }

        Vector2 input = moveAction.action.ReadValue<Vector2>();

        float x = input.x;
        float z = input.y;

        velocity.y += gravity * Time.deltaTime;


        Vector3 horizontalMove = new Vector3(x, 0f, z).normalized;
        animator.SetFloat("Speed", horizontalMove == Vector3.zero ? 0f : 1f);

        Vector3 finalMove = horizontalMove * moveSpeed + new Vector3(0f, velocity.y, 0f);
        controller.Move(finalMove * Time.deltaTime);

        RotateToMouse();
    }

    void RotateToMouse()
    {
        if (mainCamera == null || Mouse.current == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        Plane aimPlane = new Plane(Vector3.up, transform.position);

        if (aimPlane.Raycast(ray, out float distance))
        {
            Vector3 lookPoint = ray.GetPoint(distance);
            Vector3 direction = lookPoint - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotateSpeed * Time.deltaTime
                );
            }
        }
    }

    void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
    }

    void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
    }
}
