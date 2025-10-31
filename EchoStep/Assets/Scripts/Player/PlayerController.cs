using UnityEngine;
using System.Collections;
using TMPro;

public class Player : MonoBehaviour
{
    [Header("Player Parameters")]
    public float echoRecordTime = 3f;

    [Header("Dash Settings")]
    public float dashCooldown = 0.5f;
    public float dashForce = 5f;
    public float dashTime = 0.2f;
    private bool isDashing = false;
    private bool canDash = true;
    private bool canCallActivation = true;

    [Header("Player Components")]
    public EchoMechanics echoMechanics;
    public CharacterController controller;
    private MyCharacterController characterController;
    private DashEffects dashEffects;
    private SmoothCameraController smoothCameraController;

    [Header("Collectibles Count")]
    public TMP_Text energyCoresText;
    public TMP_Text dataShardsText;
    public TMP_Text modulePartsText;

    public int energyCores = 0;
    public int dataShards = 0;
    public int moduleParts = 0;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (controller == null) { controller = GetComponent<CharacterController>(); }
        characterController = GetComponent<MyCharacterController>();
        dashEffects = GetComponent<DashEffects>();
        smoothCameraController = GetComponent<SmoothCameraController>();

        UpdateHud();

        GameEventsManager.instance.collectibleEvents.onCollectibleGet += UpdateCollectiblesList;
        GameEventsManager.instance.playerEvents.onPlayerActiveEcho += ChangeEchoActivationState;
    }

    void OnDisable()
    {
        GameEventsManager.instance.collectibleEvents.onCollectibleGet -= UpdateCollectiblesList;
        GameEventsManager.instance.playerEvents.onPlayerActiveEcho -= ChangeEchoActivationState;
    }

    private void Update()
    {
        // Echo Usage
        if (Input.GetKeyDown(KeyCode.E))
        {
            canCallActivation = false;
            echoMechanics.CallEchoRecorder(transform.position, echoRecordTime);
        }

        if (Input.GetKeyDown(KeyCode.R) && canCallActivation)
        {
            canCallActivation = false;
            echoMechanics.CallEchoActivation();
        }

        // Dash Usage
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash) { StartCoroutine(PerformDash()); }

        // Pause Usage
        if (Input.GetKeyDown(KeyCode.Escape)) { GameEventsManager.instance.pauseEvents.OnPauseButtonPressed(); }
    }

    private void UpdateCollectiblesList(int id)
    {
        if (id == 1) { energyCores += 1; }
        if (id == 2) { dataShards += 1; }
        if (id == 3) { moduleParts += 1; }

        UpdateHud();
    }

    private void ChangeEchoActivationState()
    {
        canCallActivation = true;
    }

    private void UpdateHud()
    {
        energyCoresText.text = "Energy Cores: " + energyCores;
        dataShardsText.text = "Data Shards: " + dataShards;
        modulePartsText.text = "Module Parts: " + moduleParts;
    }

    /// <summary>
    /// Perform a dash
    /// </summary>
    /// <returns></returns>
    private IEnumerator PerformDash()
    {
        canDash = false;
        isDashing = true;

        if (characterController != null)
        {
            characterController.SetDashing(true);
        }

        // Start dash visual effects
        if (dashEffects != null)
        {
            dashEffects.StartDashEffects();
        }

        // Determine dash direction: use input if available, otherwise forward
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 dashDirection;

        if (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f)
        {
            // Use input direction
            dashDirection = (transform.right * x + transform.forward * z).normalized;
        }
        else
        {
            // Default to forward if no input
            dashDirection = transform.forward;
        }

        // Apply dash velocity as an impulse (adds to existing momentum, especially useful in air)
        if (characterController != null)
        {
            characterController.ApplyDashVelocity(dashDirection, dashForce);
        }

        // Keep dash active for dashTime (momentum will continue after)
        yield return new WaitForSeconds(dashTime);

        isDashing = false;
        if (characterController != null)
        {
            characterController.SetDashing(false);
        }

        // Stop dash visual effects
        if (dashEffects != null)
        {
            dashEffects.StopDashEffects();
        }

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
