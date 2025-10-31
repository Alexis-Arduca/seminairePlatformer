using UnityEngine;
using System.Collections;

public class EchoAppearance : MonoBehaviour
{
    [SerializeField] private float appearDuration = 0.6f;
    [SerializeField] private float floatHeight = 0.5f;
    [SerializeField] private ParticleSystem appearParticles; // assigner dans l’inspecteur

    private Renderer[] renderers;
    private Vector3 startPos;

    public float AppearDuration => appearDuration;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        startPos = transform.position;
    }

    private void OnEnable()
    {
        StartCoroutine(AppearRoutine());
    }

    private IEnumerator AppearRoutine()
    {
        float t = 0f;
        Vector3 targetPos = startPos;
        transform.position = startPos - Vector3.up * floatHeight;

        // 🔹 Prépare les rendus pour le fade-in
        foreach (var r in renderers)
        {
            foreach (var mat in r.materials)
            {
                Color c = mat.color;
                c.a = 0f;
                mat.color = c;
            }
        }

        // 🔹 Joue les particules si assignées
        if (appearParticles != null)
        {
            appearParticles.transform.parent = null; // détache pour qu'elles restent visibles même si l'objet bouge
            appearParticles.Play();
            Destroy(appearParticles.gameObject, appearParticles.main.duration);
        }

        // 🔹 Animation de montée + fade-in
        while (t < appearDuration)
        {
            t += Time.deltaTime;
            float normalized = t / appearDuration;

            transform.position = Vector3.Lerp(startPos - Vector3.up * floatHeight, targetPos, normalized);

            foreach (var r in renderers)
            {
                foreach (var mat in r.materials)
                {
                    Color c = mat.color;
                    c.a = Mathf.Lerp(0f, 1f, normalized);
                    mat.color = c;
                }
            }

            yield return null;
        }

        transform.position = targetPos;
    }
}
