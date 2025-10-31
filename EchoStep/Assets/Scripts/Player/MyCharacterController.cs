using UnityEngine;

[RequireComponent(typeof(UnityEngine.CharacterController))]
public class MyCharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float mouseSensitivity = 200f;
    [SerializeField] private float airControlMultiplier = 0.2f; // Very limited air control (20% of ground control)
    [SerializeField] private float groundFriction = 15f; // How quickly velocity reduces on ground when not inputting
    [SerializeField] private float airDrag = 2f; // Air resistance when in air
    private UnityEngine.CharacterController controller;
    private Player player;
    private Vector3 horizontalVelocity = Vector3.zero; // Horizontal velocity for momentum conservation
    public bool isDashing { get; private set; } = false; // Track if player is dashing

    [Header("Jump Settings")]
    public Vector3 velocity;
    public bool isGrounded;
    public bool isWalled = false;
    private bool isDoubleJump = true;
    private Vector3 wallNormal;
    [SerializeField] private float wallJumpForce = 10f;
    [SerializeField] private float wallJumpHorizontalForce = 12f;
    [SerializeField] private float wallJumpDecayRate = 0.92f;
    [SerializeField] private float maxWallDistance = 0.3f; // Maximum distance from wall to allow wall jump
    private Vector3 wallJumpDirection;
    private bool isWallJumping = false;
    private float wallJumpVelocity = 0f;
    private Vector3 lastWallPoint = Vector3.zero; // Last point of wall contact

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
            isWallJumping = false;
            wallJumpVelocity = 0f;
        }

        // Get input direction
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 inputDirection = (transform.right * x + transform.forward * z).normalized;
        float inputMagnitude = Mathf.Clamp01(new Vector2(x, z).magnitude);

        if (isGrounded)
        {
            // Ground movement: apply acceleration and friction
            Vector3 desiredVelocity = inputDirection * baseMoveSpeed * inputMagnitude;

            if (inputMagnitude > 0.1f)
            {
                // Accelerate toward desired velocity
                horizontalVelocity = Vector3.Lerp(horizontalVelocity, desiredVelocity, groundFriction * Time.deltaTime);
            }
            else
            {
                // Apply friction when no input
                horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, groundFriction * Time.deltaTime);
            }
        }
        else
        {
            // Air movement: conserve momentum with limited control
            Vector3 desiredVelocity = inputDirection * baseMoveSpeed * airControlMultiplier * inputMagnitude;

            // Reduce air drag during dash to preserve dash momentum
            float currentAirDrag = isDashing ? airDrag * 0.3f : airDrag;

            if (inputMagnitude > 0.1f)
            {
                // Limited air control - blend current velocity with input
                horizontalVelocity = Vector3.Lerp(horizontalVelocity, desiredVelocity, currentAirDrag * Time.deltaTime);
            }
            else
            {
                // Apply light air drag to slow down gradually (even less during dash)
                float dragMultiplier = isDashing ? 0.2f : 0.5f;
                horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, currentAirDrag * Time.deltaTime * dragMultiplier);
            }
        }


        // Apply horizontal velocity
        controller.Move(horizontalVelocity * Time.deltaTime);

        // Reset wall detection if grounded (wall jump only works in air)
        if (isGrounded)
        {
            isWalled = false;
        }

        // Continuously check if player is still close enough to wall using raycast
        if (isWalled && !isGrounded)
        {
            // Use raycast to check perpendicular distance to wall
            Vector3 rayOrigin = transform.position;
            Vector3 rayDirection = -wallNormal; // Cast toward the wall
            RaycastHit rayHit;

            if (Physics.Raycast(rayOrigin, rayDirection, out rayHit, maxWallDistance + 0.2f))
            {
                if (rayHit.collider.CompareTag("Wall"))
                {
                    // Check if distance is within threshold
                    if (rayHit.distance > maxWallDistance)
                    {
                        isWalled = false;
                    }
                }
                else
                {
                    // Hit something else, not a wall
                    isWalled = false;
                }
            }
            else
            {
                // No wall found in raycast, player moved away
                isWalled = false;
            }
        }

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
            return;
        }

        // Wall jump takes priority over double jump
        if (Input.GetButtonDown("Jump") && !isGrounded && isWalled)
        {
            // Calculate horizontal direction away from wall
            // hit.normal points from the collider surface outward, so use it directly for pushback
            Vector3 horizontalWallNormal = new Vector3(wallNormal.x, 0f, wallNormal.z);
            if (horizontalWallNormal.magnitude > 0.1f)
            {
                horizontalWallNormal.Normalize();
                // Use the normal directly - it should point away from the wall surface
                wallJumpDirection = horizontalWallNormal;
                wallJumpVelocity = wallJumpHorizontalForce;
                isWallJumping = true;

                // Apply wall jump pushback as an impulse to horizontal velocity
                horizontalVelocity += wallJumpDirection * wallJumpHorizontalForce;

                Debug.Log($"Wall Jump! Normal: {wallNormal}, Direction: {wallJumpDirection}, Force: {wallJumpVelocity}");
            }
            else
            {
                // Fallback: if horizontal component is too small, use wall normal directly
                wallJumpDirection = wallNormal.normalized;
                wallJumpDirection.y = 0f;
                wallJumpDirection.Normalize();
                wallJumpVelocity = wallJumpHorizontalForce;
                isWallJumping = true;

                // Apply wall jump pushback as an impulse to horizontal velocity
                horizontalVelocity += wallJumpDirection * wallJumpHorizontalForce;

                Debug.Log($"Wall Jump (fallback)! Direction: {wallJumpDirection}, Force: {wallJumpVelocity}");
            }

            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isWalled = false;
            isDoubleJump = false; // Consume double jump when wall jumping
            return;
        }

        if (Input.GetButtonDown("Jump") && !isGrounded && isDoubleJump)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isDoubleJump = false;
        }
    }

    /// <summary>
    /// Apply dash velocity - integrates with momentum system
    /// </summary>
    /// <param name="dashDirection">Direction of the dash</param>
    /// <param name="dashForce">Force of the dash</param>
    public void ApplyDashVelocity(Vector3 dashDirection, float dashForce)
    {
        if (!isGrounded)
        {
            // In air: add dash velocity to existing momentum for useful air dashes
            horizontalVelocity += dashDirection * dashForce;
        }
        else
        {
            // On ground: can either add or set velocity (adding preserves some momentum)
            horizontalVelocity += dashDirection * dashForce;
        }
    }

    /// <summary>
    /// Set dashing state (called by Player controller)
    /// </summary>
    public void SetDashing(bool dashing)
    {
        isDashing = dashing;
    }

    /// <summary>
    /// Detect collision with the wall
    /// </summary>
    /// <param name="hit"></param>
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Wall") && !isGrounded)
        {
            // Calculate perpendicular distance from controller center to wall surface
            // The normal points away from the wall, so we project the vector from center to hit point onto -normal
            Vector3 toHitPoint = hit.point - transform.position;
            float perpendicularDistance = Vector3.Dot(toHitPoint, -hit.normal);

            // Account for controller radius - subtract it to get distance to wall surface
            float controllerRadius = controller.radius;
            float distanceToWallSurface = perpendicularDistance - controllerRadius;

            // Check if the distance to the wall surface is within the threshold
            if (distanceToWallSurface >= 0 && distanceToWallSurface <= maxWallDistance)
            {
                isWalled = true;
                wallNormal = hit.normal;
                lastWallPoint = hit.point;
                Debug.Log($"Wall detected! Normal: {wallNormal}, Distance to wall: {distanceToWallSurface}");
            }
            else
            {
                // Too far from wall, don't allow wall jump
                isWalled = false;
            }
        }
    }
}
