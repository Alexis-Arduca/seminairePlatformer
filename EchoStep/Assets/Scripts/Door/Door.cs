using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The moving part. If empty, the script will look for a child named 'Door'.")]
    public Transform door;

    [Header("Motion")]
    [Tooltip("How much the door moves up in local position (meters).")]
    public float openHeight = 2f;
    [Tooltip("Movement speed (m/s).")]
    public float speed = 2f;
    [Tooltip("Does the door start open?")]
    public bool startsOpen = false;
    [Tooltip("Prevents toggling too quickly.")]
    public float cooldown = 0.4f;

    [Header("Audio (optional)")]
    public AudioClip openClip;
    public AudioClip closeClip;

    [SerializeField]
    public bool isOpenInEditor;

    Vector3 _closedLocalPos;
    Vector3 _openLocalPos;
    Coroutine _moveRoutine;
    bool _isOpen;
    float _lastToggleTime;
    AudioSource _audio;
    bool _lastEditorState;

    void Awake()
    {
        if (door == null)
        {
            var child = transform.Find("Door");
            if (child == null)
            {
                Debug.LogError("[Door] Child 'Door' not found. Assign the moving part in the inspector.");
                enabled = false;
                return;
            }
            door = child;
        }

        _closedLocalPos = door.localPosition;
        _openLocalPos   = _closedLocalPos + Vector3.up * openHeight;

        _audio = GetComponent<AudioSource>();

        if (startsOpen)
        {
            door.localPosition = _openLocalPos;
            _isOpen = true;
        }
        else
        {
            door.localPosition = _closedLocalPos;
            _isOpen = false;
        }

        _lastEditorState = isOpenInEditor;
    }

    void Update()
    {
        if (isOpenInEditor != _lastEditorState)
        {
            if (isOpenInEditor) Open();
            else Close();
            _lastEditorState = isOpenInEditor;
        }
    }

    // --- Public API ----------------------------------------------------------
    public void Toggle()
    {
        if (Time.time - _lastToggleTime < cooldown) return;
        _lastToggleTime = Time.time;

        if (_isOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (_isOpen) return;
        _isOpen = true;
        StartMove(_openLocalPos);
        Play(openClip);
    }

    public void Close()
    {
        if (!_isOpen) return;
        _isOpen = false;
        StartMove(_closedLocalPos);
        Play(closeClip);
    }

#if UNITY_EDITOR
    [ContextMenu("Door/Open")]
    public void Editor_Open()
    {
        Open();
    }

    [ContextMenu("Door/Close")]
    public void Editor_Close()
    {
        Close();
    }

    [ContextMenu("Door/Toggle")]
    public void Editor_Toggle()
    {
        Toggle();
    }
#endif

    // --- Internals -----------------------------------------------------------
    void StartMove(Vector3 target)
    {
        if (_moveRoutine != null) StopCoroutine(_moveRoutine);
        _moveRoutine = StartCoroutine(MoveTo(target));
    }

    System.Collections.IEnumerator MoveTo(Vector3 targetLocalPos)
    {
        // MoveTowards = stable, no overshoot or distance dependency.
        while ((door.localPosition - targetLocalPos).sqrMagnitude > 0.0001f)
        {
            door.localPosition = Vector3.MoveTowards(
                door.localPosition,
                targetLocalPos,
                speed * Time.deltaTime
            );
            yield return null;
        }
        door.localPosition = targetLocalPos;
        _moveRoutine = null;
    }

    void Play(AudioClip clip)
    {
        if (_audio != null && clip != null) _audio.PlayOneShot(clip);
    }

#if UNITY_EDITOR
    // Gizmos to visualize the range
    void OnDrawGizmosSelected()
    {
        Transform t = (door != null) ? door : transform.Find("Door");
        if (t == null) return;
        Gizmos.color = Color.cyan;
        Vector3 a = t.position;
        Vector3 b = a + t.TransformVector(Vector3.up * openHeight);
        Gizmos.DrawLine(a, b);
        Gizmos.DrawWireCube(b, Vector3.one * 0.1f);
    }
#endif
}