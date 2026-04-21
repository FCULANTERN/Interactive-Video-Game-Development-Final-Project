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

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        GroundCheck();
        MovePlayer();
        RotateToMouse();
    }

    void GroundCheck()
    {
        if (groundCheck != null)
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame &&
            isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void MovePlayer()
    {
        float x = 0f;
        float z = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) z += 1f;
            if (Keyboard.current.sKey.isPressed) z -= 1f;
            if (Keyboard.current.aKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed) x += 1f;
        }

        Vector3 move = new Vector3(x, 0f, z).normalized;
        controller.Move(move * moveSpeed * Time.deltaTime);
    }

    void RotateToMouse()
    {
        if (mainCamera == null || Mouse.current == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, mouseAimMask))
        {
            Vector3 lookPoint = hit.point;
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
}