using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class PressurePlate : MonoBehaviour
{
    [Header("Activation Settings")]
    [Tooltip("Can the Player activate this plate?")]
    public bool activatableByPlayer = true;

    [Tooltip("Can the Clone activate this plate?")]
    public bool activatableByClone = true;

    [Header("Events")]
    [Tooltip("Called when the plate is pressed.")]
    public UnityEvent onPressed;

    [Tooltip("Called when the plate is released.")]
    public UnityEvent onReleased;

    [Header("Audio (optional)")]
    [Tooltip("AudioSource used to play sounds. If empty, will try to use one on this GameObject.")]
    public AudioSource audioSource;
    [Tooltip("Sound played when the plate is pressed.")]
    public AudioClip pressedClip;
    [Tooltip("Sound played when the plate is released.")]
    public AudioClip releasedClip;

    [Header("Visual Feedback (optional)")]
    [Tooltip("The visual piece that moves when pressed. If empty, tries to find a child named 'Plate'.")]
    public Transform plate;
    [Tooltip("How deep the plate goes down when pressed (meters).")]
    public float pressDepth = 0.05f;
    [Tooltip("Lerp speed for the press/release movement.")]
    public float pressSpeed = 5f;

    private int _objectsOnPlate = 0;
    private bool _isPressed = false;
    Vector3 _plateStartLocalPos;
    Vector3 _platePressedLocalPos;
    Coroutine _animRoutine;

    private void Awake()
    {
        // Setup optional visuals
        if (plate == null)
        {
            var child = transform.Find("Plate");
            if (child != null) plate = child;
        }
        if (plate != null)
        {
            _plateStartLocalPos = plate.localPosition;
            _platePressedLocalPos = _plateStartLocalPos + Vector3.down * Mathf.Abs(pressDepth);
        }

        // Setup optional audio
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Ensure collider is set to trigger
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"[PressurePlate] Collider on {name} should be marked as 'Is Trigger'. Auto-fixing.");
            col.isTrigger = true;
        }
    }

    private void OnDisable()
    {
        if (_animRoutine != null)
        {
            StopCoroutine(_animRoutine);
            _animRoutine = null;
        }
    }

    private void OnDestroy()
    {
        if (_animRoutine != null)
        {
            StopCoroutine(_animRoutine);
            _animRoutine = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (CanActivate(other))
        {
            _objectsOnPlate++;
            UpdatePlateState();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (CanActivate(other))
        {
            _objectsOnPlate = Mathf.Max(0, _objectsOnPlate - 1);
            UpdatePlateState();
        }
    }

    private bool CanActivate(Collider other)
    {
        // Simplest tag-based detection; can be replaced later by an interface check
        if (other.CompareTag("Player") && activatableByPlayer) return true;
        if (other.CompareTag("Echo") && activatableByClone) return true;
        return false;
    }

    private void UpdatePlateState()
    {
        bool shouldBePressed = _objectsOnPlate > 0;

        if (shouldBePressed != _isPressed)
        {
            _isPressed = shouldBePressed;

            // Animate visual
            if (plate != null)
            {
                StartMove(_isPressed ? _platePressedLocalPos : _plateStartLocalPos);
            }

            // Play audio
            Play(_isPressed ? pressedClip : releasedClip);

            // Fire events
            if (_isPressed) onPressed?.Invoke();
            else onReleased?.Invoke();
        }
    }
#region Helpers
    void StartMove(Vector3 target)
    {
        if (!isActiveAndEnabled || plate == null) return;
        if (_animRoutine != null) StopCoroutine(_animRoutine);
        _animRoutine = StartCoroutine(AnimateTo(target));
    }

    System.Collections.IEnumerator AnimateTo(Vector3 targetLocalPos)
    {
        if (plate == null) yield break;
        while ((plate.localPosition - targetLocalPos).sqrMagnitude > 0.000001f)
        {
            if (!this || !isActiveAndEnabled || plate == null) yield break;
            plate.localPosition = Vector3.Lerp(plate.localPosition, targetLocalPos, pressSpeed * Time.deltaTime);
            yield return null;
        }
        plate.localPosition = targetLocalPos;
        _animRoutine = null;
    }

    void Play(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip);
    }
#endregion

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!this) return;
        var col = GetComponent<Collider>();
        if (col == null) return;
        Gizmos.color = _isPressed ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, col.bounds.size);
    }
#endif
}