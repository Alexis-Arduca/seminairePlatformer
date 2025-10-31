using UnityEngine;

public class SmoothCameraController : MonoBehaviour
{
    [Header("Camera Reference")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform target; // Player transform

    [Header("Mouse Look Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float verticalLookLimit = 80f; // Limit vertical rotation

    [Header("Camera Position Settings")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 2.47f, -5.67f); // Default camera position relative to player
    [SerializeField] private float positionSmoothing = 10f; // How fast camera follows player

    [Header("Movement-Based Position Offset")]
    [SerializeField] private bool enableMovementOffset = true;
    [SerializeField] private float movementOffsetAmount = 0.5f; // How much camera shifts based on movement direction
    [SerializeField] private float movementOffsetSmoothing = 8f; // How fast offset applies

    [Header("Camera Rotation Settings")]
    [SerializeField] private float rotationSmoothing = 15f; // How fast camera rotates
    [SerializeField] private bool invertY = false;
    [SerializeField] private bool lockVerticalRotation = false; // Option to lock vertical rotation

    [Header("Camera Bob Settings")]
    [SerializeField] private bool enableCameraBob = true;
    [SerializeField] private float bobAmount = 0.05f; // How much camera bobs
    [SerializeField] private float bobFrequency = 10f; // How fast camera bobs

    [Header("Camera Lean Settings")]
    [SerializeField] private bool enableCameraLean = true;
    [SerializeField] private float leanAmount = 5f; // Degrees to lean when turning
    [SerializeField] private float leanSmoothing = 8f; // How fast lean applies

    [Header("Movement-Based Effects")]
    [SerializeField] private float speedFOVMultiplier = 0.5f; // FOV increases based on speed
    [SerializeField] private float maxSpeedFOV = 10f; // Max FOV increase from speed

    [Header("Advanced Smoothing")]
    [SerializeField] private bool useSmoothDamp = true; // Use SmoothDamp instead of Lerp for ultra-smooth movement
    [SerializeField] private float smoothDampTime = 0.1f; // SmoothDamp time

    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;
    private float targetVerticalRotation = 0f;
    private float targetHorizontalRotation = 0f;
    private float verticalRotationVelocity = 0f;
    private float horizontalRotationVelocity = 0f;
    private Vector3 currentCameraPosition;
    private Vector3 velocity = Vector3.zero; // For SmoothDamp
    private Vector3 movementOffset = Vector3.zero; // Movement-based camera offset
    private Vector3 movementOffsetVelocity = Vector3.zero;
    private float bobTimer = 0f;
    private float currentLean = 0f;
    private float leanVelocity = 0f;
    private float baseFOV;

    // References
    private Transform cameraTransform;
    private MyCharacterController characterController;

    private void Start()
    {
        // Find camera if not assigned
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = FindObjectOfType<Camera>();
            }
        }

        if (playerCamera == null)
        {
            Debug.LogError("SmoothCameraController: No camera found!");
            enabled = false;
            return;
        }

        cameraTransform = playerCamera.transform;

        // Find target (player) if not assigned
        if (target == null)
        {
            target = transform;
        }

        // Get character controller for movement-based effects
        characterController = GetComponent<MyCharacterController>();
        if (characterController == null && target != null)
        {
            characterController = target.GetComponent<MyCharacterController>();
        }

        // Initialize camera position
        if (cameraTransform.parent == target || cameraTransform.parent != null)
        {
            currentCameraPosition = cameraTransform.localPosition;
        }
        else
        {
            currentCameraPosition = cameraOffset;
        }

        // Store base FOV
        baseFOV = playerCamera.fieldOfView;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void LateUpdate()
    {
        if (playerCamera == null || target == null) return;

        HandleMouseLook();
        UpdateCameraPosition();
        UpdateCameraRotation();
        UpdateCameraEffects();
    }

    /// <summary>
    /// Handle mouse look input and rotation
    /// </summary>
    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * (invertY ? 1f : -1f);

        // Update target rotations
        targetHorizontalRotation += mouseX;
        targetVerticalRotation += mouseY;

        // Clamp vertical rotation only if locked
        if (lockVerticalRotation)
        {
            targetVerticalRotation = Mathf.Clamp(targetVerticalRotation, -verticalLookLimit, verticalLookLimit);
        }

        // Smoothly interpolate rotations for ultra-smooth feel
        horizontalRotation = Mathf.SmoothDampAngle(horizontalRotation, targetHorizontalRotation, ref horizontalRotationVelocity, 1f / rotationSmoothing);
        verticalRotation = Mathf.SmoothDampAngle(verticalRotation, targetVerticalRotation, ref verticalRotationVelocity, 1f / rotationSmoothing);
    }

    /// <summary>
    /// Update camera position with smooth following
    /// </summary>
    private void UpdateCameraPosition()
    {
        Vector3 targetPosition = cameraOffset;

        // Apply camera bob if enabled
        if (enableCameraBob && characterController != null && characterController.isGrounded)
        {
            float speed = characterController.horizontalVelocity.magnitude;
            if (speed > 0.1f)
            {
                bobTimer += Time.deltaTime * bobFrequency * (speed / 5f); // Bob faster when moving faster
                float bobOffset = Mathf.Sin(bobTimer) * bobAmount * Mathf.Clamp01(speed / 5f);
                targetPosition.y += bobOffset;
            }
        }

        // Apply movement-based offset if enabled
        if (enableMovementOffset && characterController != null)
        {
            Vector3 horizontalVel = characterController.horizontalVelocity;
            horizontalVel.y = 0f;
            float speed = horizontalVel.magnitude;

            if (speed > 0.1f)
            {
                // Calculate offset direction based on movement direction (in player's local space)
                Vector3 movementDirection = horizontalVel.normalized;
                // Convert to local space relative to player's forward/right
                Vector3 localMovementDirection = target.InverseTransformDirection(movementDirection);

                // Create offset: shift camera slightly in the direction of movement
                Vector3 targetMovementOffset = new Vector3(
                    localMovementDirection.x * movementOffsetAmount,
                    0f,
                    localMovementDirection.z * movementOffsetAmount * 0.5f // Less forward/back offset
                ) * Mathf.Clamp01(speed / 5f); // Scale with speed

                // Smoothly interpolate movement offset
                movementOffset = Vector3.SmoothDamp(movementOffset, targetMovementOffset, ref movementOffsetVelocity, 1f / movementOffsetSmoothing);
            }
            else
            {
                // Smoothly return to zero when not moving
                movementOffset = Vector3.SmoothDamp(movementOffset, Vector3.zero, ref movementOffsetVelocity, 1f / movementOffsetSmoothing);
            }

            // Add movement offset to target position
            targetPosition += movementOffset;
        }

        // Smoothly move camera to target position
        if (useSmoothDamp)
        {
            if (cameraTransform.parent == target)
            {
                currentCameraPosition = Vector3.SmoothDamp(currentCameraPosition, targetPosition, ref velocity, smoothDampTime);
                cameraTransform.localPosition = currentCameraPosition;
            }
            else
            {
                Vector3 worldTargetPos = target.position + target.TransformDirection(targetPosition);
                Vector3 currentWorldPos = cameraTransform.position;
                currentWorldPos = Vector3.SmoothDamp(currentWorldPos, worldTargetPos, ref velocity, smoothDampTime);
                cameraTransform.position = currentWorldPos;
            }
        }
        else
        {
            if (cameraTransform.parent == target)
            {
                currentCameraPosition = Vector3.Lerp(currentCameraPosition, targetPosition, positionSmoothing * Time.deltaTime);
                cameraTransform.localPosition = currentCameraPosition;
            }
            else
            {
                Vector3 worldTargetPos = target.position + target.TransformDirection(targetPosition);
                cameraTransform.position = Vector3.Lerp(cameraTransform.position, worldTargetPos, positionSmoothing * Time.deltaTime);
            }
        }
    }

    /// <summary>
    /// Update camera rotation with smooth mouse look
    /// </summary>
    private void UpdateCameraRotation()
    {
        // Apply camera lean if enabled
        if (enableCameraLean && characterController != null)
        {
            Vector3 horizontalVel = characterController.horizontalVelocity;
            horizontalVel.y = 0f;
            float speed = horizontalVel.magnitude;

            if (speed > 0.1f)
            {
                // Calculate lean based on turning direction
                Vector3 forward = target.forward;
                float dot = Vector3.Dot(Vector3.Cross(forward, horizontalVel.normalized), Vector3.up);
                float targetLean = -dot * leanAmount * Mathf.Clamp01(speed / 5f);

                // Smooth lean
                currentLean = Mathf.SmoothDamp(currentLean, targetLean, ref leanVelocity, 1f / leanSmoothing);
            }
            else
            {
                currentLean = Mathf.SmoothDamp(currentLean, 0f, ref leanVelocity, 1f / leanSmoothing);
            }
        }

        // Rotate player body horizontally
        target.rotation = Quaternion.Slerp(target.rotation, Quaternion.Euler(0f, horizontalRotation, 0f), rotationSmoothing * Time.deltaTime);

        // Rotate camera vertically (relative to player)
        if (cameraTransform.parent == target)
        {
            Quaternion verticalRotationOnly = Quaternion.Euler(verticalRotation, 0f, 0f);
            Quaternion baseRotation = Quaternion.Slerp(cameraTransform.localRotation, verticalRotationOnly, rotationSmoothing * Time.deltaTime);

            // Apply lean to local rotation
            if (enableCameraLean)
            {
                Quaternion leanRotation = Quaternion.Euler(0f, 0f, currentLean);
                cameraTransform.localRotation = baseRotation * leanRotation;
            }
            else
            {
                cameraTransform.localRotation = baseRotation;
            }
        }
        else
        {
            Quaternion cameraRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
            if (enableCameraLean)
            {
                cameraRotation *= Quaternion.Euler(0f, 0f, currentLean);
            }
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, cameraRotation, rotationSmoothing * Time.deltaTime);
        }
    }

    /// <summary>
    /// Update dynamic camera effects based on movement
    /// </summary>
    private void UpdateCameraEffects()
    {
        if (characterController == null || playerCamera == null) return;

        // Speed-based FOV (only if not dashing, as dash has its own FOV effects)
        if (!characterController.isDashing)
        {
            float speed = characterController.horizontalVelocity.magnitude;
            float targetFOV = baseFOV + Mathf.Clamp(speed * speedFOVMultiplier, 0f, maxSpeedFOV);
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, 5f * Time.deltaTime);
        }
    }

    /// <summary>
    /// Set camera target (can be called externally)
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// Get current rotation values (can be used for external effects)
    /// </summary>
    public Vector2 GetRotation()
    {
        return new Vector2(horizontalRotation, verticalRotation);
    }

    /// <summary>
    /// Set rotation values (for cutscenes, etc.)
    /// </summary>
    public void SetRotation(float horizontal, float vertical)
    {
        targetHorizontalRotation = horizontal;
        horizontalRotation = horizontal;
        targetVerticalRotation = lockVerticalRotation ? Mathf.Clamp(vertical, -verticalLookLimit, verticalLookLimit) : vertical;
        verticalRotation = targetVerticalRotation;
    }
}
