using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("Player Parameters")]
    public float echoRecordTime = 3f;

    [Header("Dash Settings")]
    public float dashCooldown = 0.5f;
    public float dashForce = 5f;   // distance / vitesse du dash
    public float dashTime = 0.2f;  // durée du dash
    private bool isDashing = false;
    private bool canDash = true;

    [Header("Player Components")]
    public EchoMechanics echoMechanics;
    public CharacterController controller;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (controller == null)
            controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // Echo Usage
        if (Input.GetKeyDown(KeyCode.E)) echoMechanics.CallEchoRecorder(transform.position, echoRecordTime);
        if (Input.GetKeyDown(KeyCode.R)) echoMechanics.CallEchoActivation();

        // Dash Usage
        if (Input.GetMouseButtonDown(1) && canDash) { StartCoroutine(PerformDash()); }
    }

    /// <summary>
    /// Perform a dash
    /// </summary>
    /// <returns></returns>
    private IEnumerator PerformDash()
    {
        canDash = false;
        isDashing = true;

        Vector3 dashDirection = transform.forward;
        float elapsed = 0f;

        while (elapsed < dashTime)
        {
            controller.Move(dashDirection * dashForce * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
