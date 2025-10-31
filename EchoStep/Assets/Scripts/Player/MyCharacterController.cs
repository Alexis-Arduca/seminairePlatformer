using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public float gravity = -9.81f;
    [SerializeField] private float mouseSensitivity = 200f;
    [SerializeField] private float minPitch = -89f;
    [SerializeField] private float maxPitch = 89f;
    private float cameraPitch = 0f;

    [Header("Air Control Settings")]
    [SerializeField, Range(0f,1f)] private float airControl = 0.15f; // scale of input while airborne

    [Header("Jump/Wall Settings")]
    private bool isDoubleJump = true;
    private bool isWalled = false;
    private Vector3 wallNormal;
    [SerializeField] private float wallJumpHeight = 2.5f; // vertical boost for wall jump
    [SerializeField] private float wallJumpImpulse = 8f;  // horizontal push away from wall
    [SerializeField] private float extraMoveDecay = 8f;   // decay speed of horizontal impulse
    private Vector3 extraMove = Vector3.zero;             // transient horizontal impulse

    [Header("Dash Settings")]
    public float dashCooldown = 0.5f;
    public float dashForce = 5f;
    public float dashTime = 0.2f;
    private bool isDashing = false;
    private bool canDash = true;
    private bool canCallActivation = true;

    [Header("Dash VFX")]
    [SerializeField] private Camera playerCamera;          // Camera to apply FOV effect
    [SerializeField] private float baseFOV = 60f;          // Default FOV when not dashing
    [SerializeField] private float dashFOV = 80f;          // Target FOV during dash
    [SerializeField] private float fovLerpSpeed = 10f;     // How quickly FOV changes

    private CharacterController controller;
    private Vector3 velocity;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        if (playerCamera != null)
        {
            baseFOV = playerCamera.fieldOfView;
        }
    }

    private void Update()
    {
        // --- Mouse look (always active) ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Yaw: rotate the player
        transform.Rotate(Vector3.up * mouseX);

        // Pitch: rotate the camera locally
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }

        // --- Movement ---
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        float controlScale = controller.isGrounded ? 1f : airControl;

        if (!isDashing)
        {
            controller.Move((move * moveSpeed * controlScale + extraMove) * Time.deltaTime);
        }

        // decay any external impulse smoothly
        extraMove = Vector3.Lerp(extraMove, Vector3.zero, extraMoveDecay * Time.deltaTime);

        // Gravity always applies
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (Input.GetButtonDown("Jump"))
        {
            if (controller.isGrounded)
            {
                // Ground jump
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
                isDoubleJump = true; // reset double jump when touching ground
            }
            else if (isWalled)
            {
                // Wall jump: push away from wall and reset double jump
                Vector3 pushDir = wallNormal.normalized; // normal points away from wall
                extraMove = pushDir * wallJumpImpulse;    // horizontal impulse
                velocity.y = Mathf.Sqrt(wallJumpHeight * -2f * gravity); // vertical boost
                isWalled = false;
                isDoubleJump = true; // allow a double jump after wall jump
            }
            else if (isDoubleJump)
            {
                // Airborne double jump
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
                isDoubleJump = false;
            }
        }

        // Dash input
        if (Input.GetButtonDown("Dash") && canDash)
        {
            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        canDash = false;
        isDashing = true;

        Vector3 dashDirection = transform.forward;
        float elapsed = 0f;

        while (elapsed < dashTime)
        {
            // Move player forward
            controller.Move(dashDirection * dashForce * Time.deltaTime);

            // Ramp FOV toward dash FOV
            if (playerCamera != null)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, dashFOV, fovLerpSpeed * Time.deltaTime);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        isDashing = false;

        // Smoothly return FOV to base after dash
        if (playerCamera != null)
        {
            float t = 0f;
            // Use a short smoothing window; break early when close enough
            while (t < 1f && Mathf.Abs(playerCamera.fieldOfView - baseFOV) > 0.1f)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, baseFOV, fovLerpSpeed * Time.deltaTime);
                t += Time.deltaTime;
                yield return null;
            }
            playerCamera.fieldOfView = baseFOV;
        }

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
    private bool IsWall(Vector3 normal)
    {
        // Treat surfaces with near-horizontal normal (dot with up ~ 0) as walls
        return Mathf.Abs(Vector3.Dot(normal.normalized, Vector3.up)) < 0.2f;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Only consider walls while airborne and with appropriate tag (or remove tag check if undesired)
        if (!controller.isGrounded && IsWall(hit.normal))
        {
            isWalled = true;
            wallNormal = hit.normal;
        }
    }
}
