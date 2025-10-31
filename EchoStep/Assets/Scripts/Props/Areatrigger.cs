using UnityEngine;
using UnityEngine.Events;

public class AreaTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("Only objects with this tag will trigger events. Leave empty to trigger for all objects.")]
    [SerializeField] private string targetTag = "Player";

    [Tooltip("If true, the trigger will only fire once per object")]
    [SerializeField] private bool triggerOnce = false;

    [Header("Target Objects")]
    [Tooltip("Objects that will receive the function calls")]
    [SerializeField] private GameObject[] targetObjects;

    [Header("Enter Events")]
    [Tooltip("Method name to call on target objects when entering (optional)")]
    [SerializeField] private string onEnterMethodName = "";

    [Tooltip("Unity Event for entering (can be configured in Inspector)")]
    [SerializeField] private UnityEvent onEnterEvent;

    [Header("Exit Events")]
    [Tooltip("Method name to call on target objects when exiting (optional)")]
    [SerializeField] private string onExitMethodName = "";

    [Tooltip("Unity Event for exiting (can be configured in Inspector)")]
    [SerializeField] private UnityEvent onExitEvent;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    private System.Collections.Generic.HashSet<Collider> triggeredObjects = new System.Collections.Generic.HashSet<Collider>();

    private void Awake()
    {
        // Ensure this GameObject has a trigger collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning($"AreaTrigger on {gameObject.name} needs a Collider component! Adding BoxCollider.");
            BoxCollider boxCol = gameObject.AddComponent<BoxCollider>();
            boxCol.isTrigger = true;
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"AreaTrigger on {gameObject.name}: Collider is not set as Trigger! Setting it now.");
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if object matches target tag (or if tag check is disabled)
        if (!string.IsNullOrEmpty(targetTag) && !other.CompareTag(targetTag))
        {
            return;
        }

        // Check if this should only trigger once
        if (triggerOnce && triggeredObjects.Contains(other))
        {
            return;
        }

        if (debugLog)
        {
            Debug.Log($"[AreaTrigger] {gameObject.name}: {other.name} entered trigger area");
        }

        triggeredObjects.Add(other);

        // Invoke Unity Events
        onEnterEvent?.Invoke();

        // Call method on target objects
        if (!string.IsNullOrEmpty(onEnterMethodName))
        {
            CallMethodOnTargets(onEnterMethodName);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if object matches target tag (or if tag check is disabled)
        if (!string.IsNullOrEmpty(targetTag) && !other.CompareTag(targetTag))
        {
            return;
        }

        if (debugLog)
        {
            Debug.Log($"[AreaTrigger] {gameObject.name}: {other.name} exited trigger area");
        }

        triggeredObjects.Remove(other);

        // Invoke Unity Events
        onExitEvent?.Invoke();

        // Call method on target objects
        if (!string.IsNullOrEmpty(onExitMethodName))
        {
            CallMethodOnTargets(onExitMethodName);
        }
    }

    /// <summary>
    /// Call a method on all target objects
    /// </summary>
    private void CallMethodOnTargets(string methodName)
    {
        if (targetObjects == null || targetObjects.Length == 0)
        {
            if (debugLog)
            {
                Debug.LogWarning($"[AreaTrigger] {gameObject.name}: No target objects assigned to call method '{methodName}'");
            }
            return;
        }

        foreach (GameObject target in targetObjects)
        {
            if (target == null) continue;

            // Try to call the method on the GameObject itself
            if (CallMethod(target, methodName))
            {
                if (debugLog)
                {
                    Debug.Log($"[AreaTrigger] {gameObject.name}: Called '{methodName}' on {target.name}");
                }
                continue;
            }

            // Try to call on all MonoBehaviour components
            MonoBehaviour[] components = target.GetComponents<MonoBehaviour>();
            bool methodCalled = false;
            foreach (MonoBehaviour component in components)
            {
                if (component != null && CallMethod(component, methodName))
                {
                    methodCalled = true;
                    if (debugLog)
                    {
                        Debug.Log($"[AreaTrigger] {gameObject.name}: Called '{methodName}' on {component.GetType().Name} of {target.name}");
                    }
                    break;
                }
            }

            if (!methodCalled && debugLog)
            {
                Debug.LogWarning($"[AreaTrigger] {gameObject.name}: Method '{methodName}' not found on {target.name} or its components");
            }
        }
    }

    /// <summary>
    /// Try to call a method using reflection
    /// </summary>
    private bool CallMethod(object target, string methodName)
    {
        System.Type type = target.GetType();
        System.Reflection.MethodInfo method = type.GetMethod(methodName,
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        if (method != null)
        {
            try
            {
                // Try calling with no parameters
                if (method.GetParameters().Length == 0)
                {
                    method.Invoke(target, null);
                    return true;
                }
            }
            catch (System.Exception e)
            {
                if (debugLog)
                {
                    Debug.LogError($"[AreaTrigger] Error calling method '{methodName}': {e.Message}");
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Manually trigger enter event (useful for testing or external calls)
    /// </summary>
    public void TriggerEnter()
    {
        onEnterEvent?.Invoke();
        if (!string.IsNullOrEmpty(onEnterMethodName))
        {
            CallMethodOnTargets(onEnterMethodName);
        }
    }

    /// <summary>
    /// Manually trigger exit event (useful for testing or external calls)
    /// </summary>
    public void TriggerExit()
    {
        onExitEvent?.Invoke();
        if (!string.IsNullOrEmpty(onExitMethodName))
        {
            CallMethodOnTargets(onExitMethodName);
        }
    }

    /// <summary>
    /// Reset the trigger (useful if triggerOnce is enabled)
    /// </summary>
    public void ResetTrigger()
    {
        triggeredObjects.Clear();
    }
}
