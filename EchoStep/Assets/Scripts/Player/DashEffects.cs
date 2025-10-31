using UnityEngine;
using System.Collections;

public class DashEffects : MonoBehaviour
{
    [Header("Camera Reference")]
    [SerializeField] private Camera playerCamera;

    [Header("FOV Settings")]
    [SerializeField] private float dashFOVIncrease = 10f; // How much FOV increases during dash
    [SerializeField] private float fovSmoothingSpeed = 15f; // How fast FOV changes

    [Header("Camera Shake Settings")]
    [SerializeField] private float shakeIntensity = 0.15f; // Intensity of shake
    [SerializeField] private float shakeFrequency = 30f; // Frequency of shake (higher = more rapid)
    [SerializeField] private Vector3 shakeDirection = Vector3.one; // Which axes to shake (1,1,1 = all axes)

    [Header("Motion Blur Effect")]
    [SerializeField] private bool enableMotionBlur = false; // Optional motion blur feel

    private float baseFOV;
    private float targetFOV;
    private Vector3 originalCameraPosition;
    private bool isDashActive = false;
    private Transform cameraTransform;

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

        if (playerCamera != null)
        {
            cameraTransform = playerCamera.transform;
            baseFOV = playerCamera.fieldOfView;
            targetFOV = baseFOV;

            // Store original position - use localPosition if camera is a child, otherwise use world position relative to player
            if (cameraTransform.parent != null)
            {
                originalCameraPosition = cameraTransform.localPosition;
            }
            else
            {
                originalCameraPosition = cameraTransform.position - transform.position;
            }
        }
        else
        {
            Debug.LogWarning("DashEffects: No camera found! Dash effects will not work.");
        }
    }

    private void Update()
    {
        if (playerCamera == null) return;

        // Smoothly interpolate FOV
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, fovSmoothingSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Start dash effects (called when dash begins)
    /// </summary>
    public void StartDashEffects()
    {
        if (playerCamera == null) return;

        isDashActive = true;
        targetFOV = baseFOV + dashFOVIncrease;
        StartCoroutine(CameraShake());
    }

    /// <summary>
    /// Stop dash effects (called when dash ends)
    /// </summary>
    public void StopDashEffects()
    {
        if (playerCamera == null) return;

        isDashActive = false;
        targetFOV = baseFOV;
    }

    /// <summary>
    /// Camera shake coroutine
    /// </summary>
    private IEnumerator CameraShake()
    {
        if (cameraTransform == null) yield break;

        Vector3 originalPos = originalCameraPosition;
        bool isChildCamera = cameraTransform.parent != null;

        while (isDashActive)
        {
            // Calculate shake offset using Perlin noise for smooth random movement
            float time = Time.time * shakeFrequency;
            Vector3 shakeOffset = new Vector3(
                (Mathf.PerlinNoise(time, 0) - 0.5f) * 2f * shakeDirection.x,
                (Mathf.PerlinNoise(0, time) - 0.5f) * 2f * shakeDirection.y,
                (Mathf.PerlinNoise(time, time) - 0.5f) * 2f * shakeDirection.z
            ) * shakeIntensity;

            // Apply shake based on camera hierarchy
            if (isChildCamera)
            {
                cameraTransform.localPosition = originalPos + shakeOffset;
            }
            else
            {
                cameraTransform.position = transform.position + originalPos + shakeOffset;
            }

            yield return null;
        }

        // Reset camera position smoothly
        float elapsed = 0f;
        Vector3 startPos = isChildCamera ? cameraTransform.localPosition : cameraTransform.position - transform.position;
        float resetDuration = 0.1f;

        while (elapsed < resetDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / resetDuration;
            // Smooth ease-out
            t = 1f - (1f - t) * (1f - t);
            Vector3 currentPos = Vector3.Lerp(startPos, originalPos, t);

            if (isChildCamera)
            {
                cameraTransform.localPosition = currentPos;
            }
            else
            {
                cameraTransform.position = transform.position + currentPos;
            }

            yield return null;
        }

        // Ensure final position is exact
        if (isChildCamera)
        {
            cameraTransform.localPosition = originalPos;
        }
        else
        {
            cameraTransform.position = transform.position + originalPos;
        }
    }

    /// <summary>
    /// Reset camera position (call if needed externally)
    /// </summary>
    public void ResetCameraPosition()
    {
        if (playerCamera != null)
        {
            playerCamera.transform.localPosition = originalCameraPosition;
            playerCamera.fieldOfView = baseFOV;
            targetFOV = baseFOV;
        }
    }
}
