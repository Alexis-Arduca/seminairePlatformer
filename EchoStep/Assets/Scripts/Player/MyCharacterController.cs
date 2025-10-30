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
    public Vector3 velocity;
    public bool isGrounded;
    private Player player;

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
        }

        float moveSpeed = baseMoveSpeed;
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
