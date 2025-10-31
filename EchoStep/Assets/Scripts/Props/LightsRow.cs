using System.Collections.Generic;
using UnityEngine;

public class LightsRow : MonoBehaviour
{
    [Header("Prefab & Layout")]
    public GameObject bulbPrefab;
    [Min(1)] public int count = 5;
    public float spacing = 0.6f;
    public Vector3 startLocalPosition = Vector3.zero;
    public Vector3 direction = Vector3.right;

    [Header("Colors")]
    public Color offColor = Color.red;
    public Color onColor = Color.green;
    [Min(0f)] public float emissionIntensity = 2f;

    [Header("Debug / Testing")]
    public int debugScore;

    [Header("Energy Cores Tracking")]
    [SerializeField] private bool trackEnergyCores = false; // If true, automatically tracks player's energy cores
    [SerializeField] private Player playerReference; // Reference to player (auto-found if null)

    // ---- internals ----
    const string ContainerName = "_Bulbs";
    Transform _container;

    struct BulbParts
    {
        public GameObject go;
        public MeshRenderer renderer;
        public Light light;
        public MaterialPropertyBlock mpb;
    }
    readonly List<BulbParts> _bulbs = new();

    void Awake()
    {
        EnsureContainer();
        UpdateLayout();        // reconcile children

        // Set up energy cores tracking if enabled
        if (trackEnergyCores)
        {
            if (playerReference == null)
            {
                playerReference = FindObjectOfType<Player>();
            }

            if (playerReference != null)
            {
                // Subscribe to energy cores updates
                UpdateEnergyCores(playerReference.energyCores);
            }
        }
        else
        {
            SetScore(debugScore);  // initial state
        }
    }

    void OnDestroy()
    {
        // Unsubscribe if we were tracking
        if (trackEnergyCores && playerReference != null)
        {
            // Cleanup if needed
        }
    }

    /// <summary>
    /// Update lights based on energy cores count
    /// </summary>
    public void UpdateEnergyCores(int energyCores)
    {
        SetScore(energyCores);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
#if UNITY_EDITOR
        // Defer layout changes to avoid destroying objects inside OnValidate
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) return; // component might have been removed
            EnsureContainer();
            UpdateLayout();        // reconcile on inspector changes
            SetScore(Mathf.Max(0, debugScore));
        };
#endif
    }

    void Update()
    {
        // simple live testing in Play Mode (only if not tracking energy cores)
        if (!trackEnergyCores)
        {
            SetScore(debugScore);
        }
    }
#endif

    // ----------------- public API -----------------
    public void SetScore(int score)
    {
        if (_bulbs.Count == 0) return;
        int lit = Mathf.Clamp(score, 0, _bulbs.Count);

        for (int i = 0; i < _bulbs.Count; i++)
        {
            bool isOn = i < lit;
            ApplyColor(_bulbs[i], isOn ? onColor : offColor);
        }
    }

    // ----------------- layout logic -----------------
    void EnsureContainer()
    {
        if (_container != null) return;
        var t = transform.Find(ContainerName);
        if (t == null)
        {
            var go = new GameObject(ContainerName);
            go.hideFlags = HideFlags.DontSaveInBuild | HideFlags.HideInHierarchy;
            _container = go.transform;
            _container.SetParent(transform, false);
            _container.localPosition = Vector3.zero;
            _container.localRotation = Quaternion.identity;
            _container.localScale = Vector3.one;
        }
        else _container = t;
    }

    void UpdateLayout()
    {
        if (bulbPrefab == null) return;
        EnsureContainer();

        // Rebuild cache from children (survives domain reloads)
        RefreshBulbCache();

        // Add or remove to match 'count'
        if (_bulbs.Count < count)
        {
            int toAdd = count - _bulbs.Count;
            for (int i = 0; i < toAdd; i++) AddOne();
        }
        else if (_bulbs.Count > count)
        {
            int toRemove = _bulbs.Count - count;
            for (int i = 0; i < toRemove; i++) RemoveLast();
        }

        // Reposition all
        Vector3 dir = direction == Vector3.zero ? Vector3.right : direction.normalized;
        for (int i = 0; i < _bulbs.Count; i++)
        {
            var pos = startLocalPosition + dir * (spacing * i);
            var b = _bulbs[i];
            b.go.transform.localPosition = pos;
            b.go.transform.localRotation = Quaternion.identity;
            b.go.transform.localScale = Vector3.one;
        }
    }

    void RefreshBulbCache()
    {
        _bulbs.Clear();
        for (int i = 0; i < _container.childCount; i++)
        {
            var child = _container.GetChild(i).gameObject;
            _bulbs.Add(ExtractParts(child));
        }
    }

    void AddOne()
    {
        var go = Instantiate(bulbPrefab, _container);
        go.name = $"LightBulb ({_container.childCount - 1})";
        var parts = ExtractParts(go);
        _bulbs.Add(parts);
    }

    void RemoveLast()
    {
        int idx = _bulbs.Count - 1;
        if (idx < 0) return;
        var go = _bulbs[idx].go;
#if UNITY_EDITOR
        if (!Application.isPlaying) DestroyImmediate(go);
        else Destroy(go);
#else
        Destroy(go);
#endif
        _bulbs.RemoveAt(idx);
    }

    BulbParts ExtractParts(GameObject go)
    {
        return new BulbParts
        {
            go = go,
            renderer = go.GetComponentInChildren<MeshRenderer>(true),
            light = go.GetComponentInChildren<Light>(true),
            mpb = new MaterialPropertyBlock()
        };
    }

    void ApplyColor(BulbParts bulb, Color color)
    {
        if (bulb.renderer != null)
        {
            bulb.renderer.GetPropertyBlock(bulb.mpb);
            bulb.mpb.SetColor("_BaseColor", color);   // URP/HDRP
            bulb.mpb.SetColor("_Color", color);       // Built-in fallback
            var emission = color * Mathf.LinearToGammaSpace(emissionIntensity);
            bulb.mpb.SetColor("_EmissionColor", emission);
            bulb.renderer.SetPropertyBlock(bulb.mpb);

            var mat = bulb.renderer.sharedMaterial;
            if (mat != null && !mat.IsKeywordEnabled("_EMISSION"))
                mat.EnableKeyword("_EMISSION");
        }

        if (bulb.light != null)
        {
            bulb.light.color = color;
            bulb.light.enabled = true;
        }
    }
}
