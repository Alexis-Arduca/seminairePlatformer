using UnityEngine;

[RequireComponent(typeof(UnityEngine.CharacterController))]
public class MyCharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float mouseSensitivity = 200f;
    private UnityEngine.CharacterController controller;
    private Player player;

    [Header("Jump Settings")]
    public Vector3 velocity;
    public bool isGrounded;
    public bool isWalled = false;
    private bool isDoubleJump = true;
    private Vector3 wallNormal;
    private float wallJumpForce = 10f;

    private void Start()
    {
        controller = GetComponent<UnityEngine.CharacterController>();
        if (controller == null)
        {
            Debug.LogError("CharacterController component missing on " + gameObject.name);
        }

        player = GetComponent<Player>();
        if (player == null)
        {
            Debug.LogError("Player component missing on " + gameObject.name);
        }
    }

    private void Update()
    {
        if (controller == null || player == null) return;

        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            isDoubleJump = true;
        }

        float moveSpeed = baseMoveSpeed;
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);

        JumpManager();

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// Manage all Jumps (Normal Jump, Double Jump and Wall Jump)
    /// </summary>
    private void JumpManager()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        if (Input.GetButtonDown("Jump") && !isGrounded && isDoubleJump)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isDoubleJump = false;
        }

        if (Input.GetButtonDown("Jump") && !isGrounded && isWalled)
        {
            

            Vector3 wallDirection = -wallNormal;
            controller.Move(wallDirection * wallJumpForce * Time.deltaTime);
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            isWalled = false;
        }
    }

    /// <summary>
    /// Detect collision with the wall
    /// </summary>
    /// <param name="hit"></param>
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Wall")) { isWalled = true; wallNormal = hit.normal; }
    }
}
